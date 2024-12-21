
using System.Threading.Tasks;
using AISmart.Options;
using AISmart.Provider;
using Microsoft.Extensions.Options;
using Xunit;

namespace AISmart.Rag;

public class EmbeddingIntegrationTests : AISmartApplicationTestBase
{
    
    [Fact]
    public async Task TestEmbedding()
    {
        var config = GetRequiredService<IOptionsMonitor<RagOptions>>().CurrentValue;
        var embeddingProvider = new OpenAIEmbeddingProvider(config.APIKey);
        
        string textContent = "Test chunk content.";
        
        float[] results = await embeddingProvider.GetEmbeddingAsync(textContent);

        var s = 1;
    }
}