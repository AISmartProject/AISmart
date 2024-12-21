using System.Text.Json;
using System.Text.Json.Serialization;
using AutoGen.Core;

namespace AISmart.AutoGen.Tests.Common;

public class DexExchange
{
    /// <summary>
    /// Get token price
    /// </summary>
    /// <param name="symbol">token symbol</param>
    /// <returns>token price</returns>
    [Function]
    public async Task<string> GetTokenPriceAsync(string symbol)
    {
        return "$2.0";
    }
    
    public FunctionContract GetTokenPriceAsyncContract
    {
        get => new FunctionContract
        {
            ClassName = @"DexExchange",
            Name = nameof(GetTokenPriceAsync),
            Description = @"get symbol price",
            ReturnType = typeof(Task<string>),
            ReturnDescription = "symbol price",
            Parameters = new global::AutoGen.Core.FunctionParameterContract[]
            {
                new FunctionParameterContract
                {
                    Name = @"symbol",
                    Description = @"token symbol",
                    ParameterType = typeof(string),
                    IsRequired = true,
                },
            },
        };
    }
    
    public Task<string> GetTokenPriceAsyncWrapper(string arguments)
    {
        var schema = JsonSerializer.Deserialize<BuyTokenSchema>(
            arguments,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

        return GetTokenPriceAsync(schema.symbol);
    }
    
    private class BuyTokenSchema
    {
        [JsonPropertyName(@"symbol")] public string symbol { get; set; }
    }
}