using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AISmart.Options;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Orleans;
using Orleans.Providers;

namespace AISmart.Grains;

[StorageProvider(ProviderName = "PubSubStore")]
public class ChatAgentGrain : Grain, IChatAgentGrain
{
    private MiddlewareStreamingAgent<OpenAIChatAgent>? _agent;
    private readonly MicroAIOptions _options;
    private readonly ILogger<ChatAgentGrain> _logger;

    public ChatAgentGrain(IOptions<MicroAIOptions> options, ILogger<ChatAgentGrain> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<MicroAIMessage?> SendAsync(string message, List<MicroAIMessage>? chatHistory)
    {
        if (_agent != null)
        {
            var history = ConvertMessage(chatHistory);
            var imMessage = await _agent.SendAsync(message, history);
            return new MicroAIMessage("assistant", imMessage.GetContent()!);
        }

        _logger.LogWarning($"[ChatAgentGrain] Agent is not set");
        return null;
    }

    public Task SetAgentAsync(string systemMessage)
    {
        var agentName = this.GetPrimaryKeyString();
        var client = new ChatClient(_options.Model, _options.ApiKey);

        _agent = new OpenAIChatAgent(client, agentName, systemMessage)
            .RegisterMessageConnector();
        return Task.CompletedTask;
    }

    public Task SetAgentWithTemperature(string systemMessage, float temperature, int? seed = null,
        int? maxTokens = null)
    {
        var agentName = this.GetPrimaryKeyString();
        var client = new ChatClient(_options.Model, _options.ApiKey);

        _agent = new OpenAIChatAgent(client, agentName, systemMessage, temperature, maxTokens, seed)
            .RegisterMessageConnector();
        return Task.CompletedTask;
    }

    private List<IMessage> ConvertMessage(List<MicroAIMessage> listAutoGenMessage)
    {
        var result = new List<IMessage>();
        foreach (var item in listAutoGenMessage)
        {
            result.Add(new TextMessage(GetRole(item.Role), item.Content));
        }

        return result;
    }

    private Role GetRole(string roleName)
    {
        switch (roleName)
        {
            case "user":
                return Role.User;
            case "assistant":
                return Role.Assistant;
            case "system":
                return Role.System;
            case "function":
                return Role.Function;
            default:
                return Role.User;
        }
    }
}