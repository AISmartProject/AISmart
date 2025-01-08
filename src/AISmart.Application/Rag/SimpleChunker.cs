using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.AI.TextAnalytics;

namespace AISmart.Rag;

public class SimpleChunker : IChunker
{
    
    private readonly TextAnalyticsClient _client;
    
    public SimpleChunker(string endpoint, string apiKey)
    {
        var credentials = new AzureKeyCredential(apiKey);
        _client = new TextAnalyticsClient(new Uri(endpoint), credentials);
    }

    /**
     * Assisted segmentation based on natural language understanding
     */
    public async Task<List<string>> SmartChunkTextAsync(string text, int maxChunkSize)
    {
        var chunks = new List<string>();
        var keyPhrases = await _client.ExtractKeyPhrasesAsync(text);

        StringBuilder currentChunk = new StringBuilder();

        foreach (var phrase in keyPhrases.Value)
        {
            if (currentChunk.Length + phrase.Length > maxChunkSize)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
            }

            currentChunk.Append($"{phrase}. ");
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }

        return chunks;
    }
    
    /**
     * the basic character number chunking logic 
     */
    public async Task<List<string>> BaseChunk(string text, int chunkSize)
    {
        var chunks = new List<string>();
        int length = text.Length;

        for (int i = 0; i < length; i += chunkSize)
        {
            int actualChunkSize = chunkSize;
            if (i + chunkSize < length && char.IsLetterOrDigit(text[i + chunkSize]))
            {
                while (i + actualChunkSize < length && char.IsLetterOrDigit(text[i + actualChunkSize]))
                {
                    actualChunkSize++;
                }
            }

            chunks.Add(text.Substring(i, Math.Min(actualChunkSize, length - i)));
        }
        return chunks;
    }
}