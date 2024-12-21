using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Options;
using AISmart.Provider;
using Microsoft.Extensions.Options;
using Xunit;

namespace AISmart.Rag;

public class QdrantIntegrationTests : AISmartApplicationTestBase
{
    [Fact]
    public async Task TestStoreAndRetrieve()
    {
        var config = GetRequiredService<IOptionsMonitor<RagOptions>>().CurrentValue;
        var qdrantDatabase = new QdrantVectorDatabase(config.QdrantUrl, config.CollectionName, config.VectorSize);
        
        string textContent = "Test chunk content.";
        float[] testVector = new float[] { 0.1f, 0.2f, 0.3f };
        
        await qdrantDatabase.StoreAsync(textContent, testVector);
        
        List<string> results = await qdrantDatabase.RetrieveAsync(testVector);
        
        Assert.Contains(textContent, results);
    }
    
    [Fact]
    public async Task StoreBatchAsync_ShouldStoreAndRetrieveData()
    {
        var config = GetRequiredService<IOptionsMonitor<RagOptions>>().CurrentValue;
        var vectorDatabase = new QdrantVectorDatabase(config.QdrantUrl, config.CollectionName, config.VectorSize);
        
        var points = new List<(float[] vector, string text)>
        {
            ( new float[] { 0.1f, 0.3f, 0.1f }, "Text 1"),
            ( new float[] { 0.2f, 0.1f, 0.2f }, "Text 2"),
            ( new float[] { 0.4f, 0.3f, 0.2f }, "Text 3")
        };
        
        await vectorDatabase.StoreBatchAsync(points);
        
        foreach (var ( vector, originalText) in points)
        {
            var retrievedTexts = await vectorDatabase.RetrieveAsync(vector, 1);
            Assert.Contains(originalText, retrievedTexts);
        }
    }
}