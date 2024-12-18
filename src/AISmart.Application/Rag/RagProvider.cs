using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AISmart.Rag;

public class RagProvider : IRagProvider
{
    private readonly IChunker _chunker;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorDatabase _vectorDatabase;

    public RagProvider()
    {
        _chunker = new SimpleChunker();
        _embeddingProvider = new OpenAIEmbeddingProvider();
        _vectorDatabase = new QdrantVectorDatabase();
    }

    public async Task StoreTextAsync(string text)
    {
        var chunks = await _chunker.Chunk(text, 4000);
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
            var chunks = await _chunker.Chunk(text, 4000);
                
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
    
    private async Task BatchStoreFilesAsync(IList<string> files)
    {
        var texts = new List<string>();
        foreach (var file in files)
        {
            string text;

            try
            {
                text = File.ReadAllText(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {file}: {ex.Message}");
                continue;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine($"No text available in file: {file}");
                continue;
            }
            
            texts.Add(text);
        }
        
        await BatchStoreTextsAsync(texts);
    }
}