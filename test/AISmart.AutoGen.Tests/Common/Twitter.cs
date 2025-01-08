using System.Text.Json;
using System.Text.Json.Serialization;
using AutoGen.Core;

namespace AISmart.AutoGen.Tests.Common;

public class Twitter
{
    /// <summary>
    /// get twitter news.
    /// </summary>
    [Function]
    public async Task<string> GetLastNews()
    {
        return "BTC 10W";
    }

    /// <summary>
    /// send message to twitter
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [Function]
    public async Task<string> SendTwitterMessage(string message)
    {
        return await Task.FromResult(message);
    }

    public FunctionContract GetTwitterNewsContract
    {
        get => new FunctionContract
        {
            ClassName = @"Twitter",
            Name = @"GetLastNews",
            Description = @"get twitter news.",
            ReturnType = typeof(Task<string>),
            Parameters = new global::AutoGen.Core.FunctionParameterContract[]
            {
                // new FunctionParameterContract
                // {
                //     Name = @"string",
                //     Description = @"strings to concatenate",
                //     ParameterType = typeof(string),
                //     IsRequired = true,
                // },
            },
        };
    }

    public FunctionContract SendTwitterMessageContract
    {
        get => new FunctionContract
        {
            ClassName = @"Twitter",
            Name = @"SendTwitterMessage",
            Description = @"send twitter message",
            ReturnType = typeof(Task<string>),
            Parameters = new global::AutoGen.Core.FunctionParameterContract[]
            {
                new FunctionParameterContract
                {
                    Name = @"message",
                    Description = @"twitter message",
                    ParameterType = typeof(string),
                    IsRequired = true,
                },
            },
        };
    }

    public Task<string> GetLastNewsWrapper(string arguments)
    {
        return GetLastNews();
    }

    public Task<string> SendTwitterMessageWrapper(string arguments)
    {
        var schema = JsonSerializer.Deserialize<SendTwitterMessageSchema>(
            arguments,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

        return SendTwitterMessage(schema.message);
    }

    private class SendTwitterMessageSchema
    {
        [JsonPropertyName(@"message")] public string message { get; set; }
    }
}