using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Rag.Agent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace AISmart.Rag;


public class ChunkerTests : AISmartApplicationTestBase
{
    
    private readonly string text = "Retrieval-Augmented Generation (RAG) is a technique that enhances language model generation by incorporating external knowledge" +
    "Yet while many critics are dismayed at the prospect, few should be surprised given the influence the kingdom's unprecedented investment in sport has secured." +
    "With RAG, the LLM is able to leverage knowledge and information that is not necessarily in its weights by providing it access to external knowledge sources such as databases" +
    "Fifa has defended a fast-tracked process that many argue was lacking in transparency and accountability" +
    "It leverages a retriever to find relevant contexts to condition the LLM, in this way, RAG is able to augment the knowledge-base of an LLM with relevant document" +
    "So is the tournament being used to help transform Saudi Arabia's reputation, or can it be a catalyst for social reform? And what does this tell us about Fifa and football more widely?";
    
    [Fact]
    public async Task TestSimpleChunker()
    {
        var simpleChunker = new SimpleChunker();

        Task<List<string>> result = simpleChunker.Chunk(text, 100);
            
        List<string> chunks = await result;
        Assert.True(chunks.Count > 0);
    }
    
    [Fact]
    public async Task TestAzureAIChunker()
    {
        var azureAiChunker = new AzureAIChunker();

        Task<List<string>> result = azureAiChunker.Chunk(text, 100);
            
        List<string> chunks = await result;
        Assert.True(chunks.Count > 0);
    }
    
    [Fact]
    public async Task TestOpenAIChunk()
    {
        var openAiChunker = new OpenAIChunker();

        Task<List<string>> result = openAiChunker.Chunk(text, 512);
            
        List<string> chunks = await result;
        Assert.True(chunks.Count > 0);
    }
    
}