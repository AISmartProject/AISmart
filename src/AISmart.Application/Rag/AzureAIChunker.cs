using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AISmart.Rag;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Configuration;

namespace AISmart.Rag;

public class AzureAIChunker : IChunker
{
    private readonly TextAnalyticsClient _client;

    public AzureAIChunker()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        IConfiguration config = builder.Build();

        var resolvedEndpoint = config["AzureAI:Endpoint"];
        var resolvedApiKey = config["AzureAI:ApiKey"];

        if (string.IsNullOrEmpty(resolvedEndpoint) || string.IsNullOrEmpty(resolvedApiKey))
        {
            throw new InvalidOperationException("Open AI Language Service Endpoint and API Key must be provided.");
        }

        var credentials = new AzureKeyCredential(resolvedApiKey);
        _client = new TextAnalyticsClient(new Uri(resolvedEndpoint), credentials);
    }

    public async Task<List<string>> Chunk(string text, int maxChunkSize)
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
    
    
}