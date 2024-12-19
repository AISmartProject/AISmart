using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AISmart.Embedding;
using AISmart.Embedding.Dto;
using AISmart.Rag;
using Newtonsoft.Json;
using Volo.Abp.DependencyInjection;

namespace AISmart.Provider;

public class OpenAIEmbeddingProvider : IEmbeddingProvider, ISingletonDependency
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public OpenAIEmbeddingProvider(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var requestBody = new { input = text, model = "text-embedding-ada-002" };
        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/embeddings", content);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<EmbedResponse>(responseString).Data[0].Embedding;
    }
}