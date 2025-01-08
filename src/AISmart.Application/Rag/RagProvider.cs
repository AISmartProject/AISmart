using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AISmart.Rag;

public class RagProvider : IRagProvider
{
    private readonly IChunker _chunker;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorDatabase _vectorDatabase;

    public RagProvider(IChunker chunker, IEmbeddingProvider embeddingProvider, IVectorDatabase vectorDatabase)
    {
        _chunker = chunker;
        _embeddingProvider = embeddingProvider;
        _vectorDatabase = vectorDatabase;
    }

    public async Task StoreTextAsync(string text)
    {
        var chunks = await _chunker.SmartChunkTextAsync(text, 4000);
        foreach (var chunk in chunks)
        {
            var embedding = await _embeddingProvider.GetEmbeddingAsync(chunk);
            await _vectorDatabase.StoreAsync(chunk, embedding);
        }
    }
    
    public async Task BatchStoreTextsAsync(IEnumerable<string> texts)
    {
        var allChunks = new List<(float[] vector, string text)>();
            
        foreach (var text in texts)
        {
            var chunks = await _chunker.SmartChunkTextAsync(text, 4000);
                
            foreach (var chunk in chunks)
            {
                var embedding = await _embeddingProvider.GetEmbeddingAsync(chunk);
                allChunks.Add((embedding, chunk));
            }
        }
        
        await _vectorDatabase.StoreBatchAsync(allChunks);
    }

    public async Task<string> RetrieveAnswerAsync(string query)
    {
        var queryEmbedding = await _embeddingProvider.GetEmbeddingAsync(query);
        var relevantChunks = await _vectorDatabase.RetrieveAsync(queryEmbedding, 3);
        return string.Join(" ", relevantChunks);
    }
}