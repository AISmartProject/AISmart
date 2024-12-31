using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AISmart.Options;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Volo.Abp.DependencyInjection;

namespace AISmart.Provider;

public class AIAgentProvider : IAIAgentProvider, ISingletonDependency
{
    private readonly MicroAIOptions _options;
    private readonly ChatClient _chatClient;

    public AIAgentProvider(IOptions<MicroAIOptions> options)
    {
        _options = options.Value;
        _chatClient = new ChatClient(_options.Model, _options.ApiKey);
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

    public async Task<MicroAIMessage?> SendAsync(MiddlewareStreamingAgent<OpenAIChatAgent> agent, string message, List<MicroAIMessage>? chatHistory)
    {
        var history = ConvertMessage(chatHistory);
        var imMessage = await agent.SendAsync(message, history);
        return new MicroAIMessage("assistant", imMessage.GetContent()!);
    }

    public async Task<MiddlewareStreamingAgent<OpenAIChatAgent>> GetAgentAsync(string agentName, string systemMessage)
    {
        return new OpenAIChatAgent(_chatClient, agentName, systemMessage)
            .RegisterMessageConnector();
    }
}