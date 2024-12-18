using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenAI.API;

namespace AISmart.Rag;

public class OpenAIChunker : IChunker
{
    
    private readonly string? _apiKey;
    
    private readonly int _maxTokensPerChunk;
    
    private readonly double _temperature;

    public OpenAIChunker()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        IConfiguration config = builder.Build();

        _apiKey = config["OpenAI:Endpoint"];
        _maxTokensPerChunk = int.Parse(config["OpenAI:MaxTokensPerChunk"] ?? string.Empty);
        _temperature = double.Parse(config["OpenAI:Temperature"] ?? string.Empty);

        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("Open AI API Key must be provided.");
        }
    }

    public async Task<List<string>> Chunk(string text, int maxChunkSize)
    {
        var results = new List<string>();
        
        // Smart Blockchain: Maximum Token Limit (assuming a maximum of 512 tokens per block)
        var chunks = ChunkText(text, _maxTokensPerChunk);
        
        // Call OpenAI API for each chunk (e.g. generate summary)
        foreach (var chunk in chunks)
        {
            string summary = GenerateSummary(chunk).Result;
            results.Add(summary);
        }

        return results;
    }
    
    private List<string> ChunkText(string text, int maxTokens)
    {
        var chunks = new List<string>();
        int start = 0;

        // Character-based chunking can later be replaced with a token-based approach
        while (start < text.Length)
        {
            // Assume that each character corresponds to 1 token on average (need to use tokenizer to verify)
            var length = Math.Min(maxTokens, text.Length - start);
            var chunk = text.Substring(start, length);

            // Try to end at a semantic boundary (e.g. a period)
            var lastPeriod = chunk.LastIndexOfAny(new[] { '.', '!', '?' });
            if (lastPeriod != -1 && start + lastPeriod + 1 < text.Length)
            {
                chunk = text.Substring(start, lastPeriod + 1);
                start += lastPeriod + 1;
            }
            else
            {
                start += length;
            }

            chunks.Add(chunk.Trim());
        }

        return chunks;
    }
    
    private async Task<string> GenerateSummary(string text)
    {
        // Initialize the OpenAI API client
        var openai = new OpenAIAPI(_apiKey);

        var prompt = $"Summarize the following text:\n\n{text}";

        var response = await openai.Completions.CreateCompletionAsync(
            prompt,
            max_tokens: _maxTokensPerChunk, 
            temperature: _temperature);

        return response.Completions[0].Text;
    }
}