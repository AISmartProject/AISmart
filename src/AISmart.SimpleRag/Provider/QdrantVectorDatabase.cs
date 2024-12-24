using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using AISmart.Rag;
using AISmart.VectorStorage;
using AISmart.VectorStorage.Dto;
using Newtonsoft.Json;
using Volo.Abp.DependencyInjection;

namespace AISmart.Provider;

public class QdrantVectorDatabase : IVectorDatabase, ISingletonDependency
{
    private readonly string _qdrantUrl;
    private readonly HttpClient _httpClient;
    private readonly string _collectionName;
    private const int DefaultVectorSize = 1536;
    private readonly int _vectorSize;

    public QdrantVectorDatabase(string url, string collectionName, int vectorSize)
    {
        _qdrantUrl = url;
        _collectionName = collectionName;
        _vectorSize = vectorSize;
        _httpClient = new HttpClient();
    }
    
    private async Task EnsureCollectionExistsAsync()
    {
        var vectorSize = _vectorSize == 0 ? DefaultVectorSize : _vectorSize;
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
        await EnsureCollectionExistsAsync();
        
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
        await EnsureCollectionExistsAsync();
        
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
        await EnsureCollectionExistsAsync();
        var requestBody = new { vector = queryEmbedding, top = topK, with_payload = true };
        var response = await _httpClient.PostAsJsonAsync($"{_qdrantUrl}/collections/{_collectionName}/points/search", requestBody);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var results = JsonConvert.DeserializeObject<SearchResponse>(responseString);
        return results.Result.Select(r => r.Payload.Text).ToList();
    }
}