using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AISmart.Rag;

public class QdrantIntegrationTests
{
    private readonly string _qdrantUrl = "http://localhost:6333";  
    private readonly string _collectionName = "test_batch_collection";

    [Fact]
    public async Task TestStoreAndRetrieve()
    {
        var qdrantDatabase = new QdrantVectorDatabase();
        
        string textContent = "Test chunk content.";
        float[] testVector = new float[] { 0.1f, 0.2f, 0.3f };
        
        await qdrantDatabase.StoreAsync(textContent, testVector);
        
        List<string> results = await qdrantDatabase.RetrieveAsync(testVector);
        
        Assert.Contains(textContent, results);
    }
    
    [Fact]
    public async Task StoreBatchAsync_ShouldStoreAndRetrieveData()
    {
        var vectorDatabase = new QdrantVectorDatabase();

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