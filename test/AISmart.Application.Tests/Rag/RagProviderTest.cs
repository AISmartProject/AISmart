using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Options;
using AISmart.Provider;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace AISmart.Rag;


public class RagProviderTest : AISmartApplicationTestBase
{
    [Fact]
    public async Task StoreBatchAsync_Test()
    {
        var config = GetRequiredService<IOptionsMonitor<RagOptions>>();
        var logger = GetRequiredService<ILogger<RagProvider>>();
        var ragProvider = new RagProvider(config, logger);

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
       
        var answer = await ragProvider.RetrieveAnswerAsync(question);
        Assert.Contains(keyword, answer);
    }
    
}