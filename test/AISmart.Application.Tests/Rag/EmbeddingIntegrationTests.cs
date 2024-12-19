using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace AISmart.Rag;

public class EmbeddingIntegrationTests : AISmartApplicationTestBase
{
    
    [Fact]
    public async Task TestEmbedding()
    {
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        var apikey = configuration["Rag:APIKey"];
        var embeddingProvider = new OpenAIEmbeddingProvider();
        
        string textContent = "Test chunk content.";
        
        float[] results = await embeddingProvider.GetEmbeddingAsync(textContent);

        var s = 1;
    }
}