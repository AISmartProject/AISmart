using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Rag.Agent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace AISmart.Rag;


public class RagProviderTest : AISmartApplicationTestBase
{
    private readonly string _qdrantUrl = "http://localhost:6333"; 
    private readonly string _collectionName = "test_collection"; 
    
    [Fact]
    public async Task StoreBatchAsync_Test()
    {
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        var apiKey = configuration["Rag:APIKey"];
        var endpoint = configuration["AzureService:endpoint"];
        var azureKey = configuration["AzureService:apiKey"];
        
        var chunker = new SimpleChunker(endpoint, azureKey); 
        var embeddingProvider = new OpenAIEmbeddingProvider(apiKey); 
        var vectorDatabase = new QdrantVectorDatabase(_qdrantUrl, _collectionName);

        var ragProvider = new RagProvider(chunker, embeddingProvider, vectorDatabase);

        var texts = new List<string>
        {
            "Retrieval-Augmented Generation (RAG) is a technique that enhances language model generation by incorporating external knowledge",
            "Yet while many critics are dismayed at the prospect, few should be surprised given the influence the kingdom's unprecedented investment in sport has secured.",
            "With RAG, the LLM is able to leverage knowledge and information that is not necessarily in its weights by providing it access to external knowledge sources such as databases",
            "Fifa has defended a fast-tracked process that many argue was lacking in transparency and accountability",
            "It leverages a retriever to find relevant contexts to condition the LLM, in this way, RAG is able to augment the knowledge-base of an LLM with relevant documents",
            "So is the tournament being used to help transform Saudi Arabia's reputation, or can it be a catalyst for social reform? And what does this tell us about Fifa and football more widely?"
        };
        
        await ragProvider.BatchStoreTextsAsync(texts);

        var keyword = "RAG";
        var question = "what is " + keyword;
        var queryEmbedding = await embeddingProvider.GetEmbeddingAsync(question);
        var answer = await vectorDatabase.RetrieveAsync(queryEmbedding,3);
        
        foreach (var chunk in answer)
        {
            Assert.Contains(keyword, chunk);
        }
    }
    
}