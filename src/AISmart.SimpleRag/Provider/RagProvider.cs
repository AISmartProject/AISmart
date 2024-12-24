using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AISmart.Chunk;
using AISmart.Embedding;
using AISmart.Options;
using AISmart.Rag;
using AISmart.VectorStorage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace AISmart.Provider;

public class RagProvider : IRagProvider, ISingletonDependency
{
    private readonly IChunker _chunker;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorDatabase _vectorDatabase;
    private readonly ILogger<RagProvider> _logger;
    private readonly IOptions<RagOptions> _ragOptions;

    public RagProvider(IOptions<RagOptions> ragOptions, ILogger<RagProvider> logger)
    {
        _ragOptions = ragOptions;
        _chunker = new SimpleChunker();
        _embeddingProvider = new OpenAIEmbeddingProvider(_ragOptions.Value.APIKey);
        _vectorDatabase = new QdrantVectorDatabase(_ragOptions.Value.QdrantUrl, 
            _ragOptions.Value.CollectionName, 
            _ragOptions.Value.VectorSize);
        _logger = logger;
    }

    public async Task StoreTextAsync(string text)
    {
        _logger.LogInformation("store text {text}", text);
        var chunkSize = _ragOptions.Value.ChunkSize;
        var chunks = await _chunker.Chunk(text, chunkSize);
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
        _logger.LogInformation("retrieve text {query}", query);
        var queryEmbedding = await _embeddingProvider.GetEmbeddingAsync(query);
        var relevantChunks = await _vectorDatabase.RetrieveAsync(queryEmbedding, 3);
        return relevantChunks.IsNullOrEmpty() ? "" : string.Join(" ", relevantChunks);
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