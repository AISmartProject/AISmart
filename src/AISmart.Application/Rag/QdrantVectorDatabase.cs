using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AISmart.Rag;

public class QdrantVectorDatabase : IVectorDatabase
{
    private readonly string _qdrantUrl;
    private readonly HttpClient _httpClient;
    private readonly string _collectionName;
    private const int DefaultVectorSize = 1536;

    public QdrantVectorDatabase()
    {
        // _qdrantUrl = qdrantUrl;
        // _collectionName = collectionName;
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        IConfiguration config = builder.Build();
        _qdrantUrl = config["Rag:QdrantUrl"];;
        _collectionName = config["Rag:CollectionName"];;
        _httpClient = new HttpClient();
    }
    
    private async Task EnsureCollectionExistsAsync(int vectorSize)
    {
        var existingCollections = await GetCollectionsAsync();
        if (!existingCollections.Contains(_collectionName))
        {
            await CreateCollectionAsync(vectorSize);
        }
    }
    
    private async Task<string[]> GetCollectionsAsync()
    {
        var response = await _httpClient.GetAsync($"{_qdrantUrl}/collections");
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<CollectionsResponse>(data);
        return result.Result.Collections.Select(c => c.Name).ToArray();
    }
    
    private async Task CreateCollectionAsync(int vectorSize)
    {
        var requestBody = new
        {
            vectors = new { size = vectorSize, distance = "Cosine" }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"{_qdrantUrl}/collections/{_collectionName}", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task StoreAsync(string chunk, float[] embedding)
    {
        await EnsureCollectionExistsAsync(DefaultVectorSize);
        
        var requestBody = new
        {
            points = new[]
            {
                new
                {
                    id = Guid.NewGuid().ToString(),
                    vector = embedding,
                    payload = new { text = chunk }
                }
            }
        };
        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{_qdrantUrl}/collections/{_collectionName}/points?wait=true", content);
        response.EnsureSuccessStatusCode();
    }
    
    public async Task StoreBatchAsync(IEnumerable<(float[] vector, string text)> points)
    {
        await EnsureCollectionExistsAsync(DefaultVectorSize);
        
        var requestBody = new
        {
            points = points.Select(p => new
            {
                id = Guid.NewGuid().ToString(),
                vector = p.vector,
                payload = new { text = p.text }
            }).ToArray()
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"{_qdrantUrl}/collections/{_collectionName}/points", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error storing batch vectors: {response.StatusCode} - {errorContent}");
            response.EnsureSuccessStatusCode();
        }
    }

    public async Task<List<string>> RetrieveAsync(float[] queryEmbedding, int topK = 5)
    {
        var requestBody = new { vector = queryEmbedding, top = topK, with_payload = true };
        var response = await _httpClient.PostAsJsonAsync($"{_qdrantUrl}/collections/{_collectionName}/points/search", requestBody);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var results = JsonConvert.DeserializeObject<SearchResponse>(responseString);
        return results.Result.Select(r => r.Payload.Text).ToList();
    }
    
    private class CollectionsResponse
    {
        [JsonProperty("result")]
        public CollectionResult Result { get; set; }
    }
    
    private class CollectionResult
    {
        [JsonProperty("collections")]
        public List<CollectionItem> Collections { get; set; }
    }
    
    private class CollectionItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    private class SearchResponse
    {
        public List<Result> Result { get; set; }
    }

    private class Result
    {
        public Payload Payload { get; set; }
    }

    private class Payload
    {
        public string Text { get; set; }
    }
}