using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.AI.TextAnalytics;

public class AzureTextAnalyticsHelper
{
    private readonly TextAnalyticsClient _client;

    public AzureTextAnalyticsHelper(string? endpoint, string apiKey)
    {
        var credentials = new AzureKeyCredential(apiKey);
        _client = new TextAnalyticsClient(new Uri(endpoint), credentials);
    }

    public TextAnalyticsClient GetClient()
    {
        return _client;
    }
    
    /**
     * Assisted segmentation based on natural language understanding
     */
    public async Task<List<string>> SmartChunkTextAsync(string text, int maxChunkSize)
    {
        var chunks = new List<string>();
        var keyPhrases = await _client.ExtractKeyPhrasesAsync(text);

        string currentChunk = string.Empty;

        foreach (var phrase in keyPhrases.Value)
        {
            if (currentChunk.Length + phrase.Length > maxChunkSize)
            {
                chunks.Add(currentChunk.Trim());
                currentChunk = string.Empty;
            }

            currentChunk += $"{phrase}. ";
        }

        if (!string.IsNullOrEmpty(currentChunk))
        {
            chunks.Add(currentChunk.Trim());
        }

        return chunks;
    }
    
    /**
     * the basic character number chunking logic 
     */
    public static List<string> SplitText(string text, int chunkSize)
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