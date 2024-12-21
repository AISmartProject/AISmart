using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.AgentTask;
using AISmart.Application.Grains.Event;
using AISmart.Dapr;
using AISmart.Domain.Grains.Event;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI;
using Orleans;

namespace AISmart.Mock;

public class MockDaprProvider :  IDaprProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _twitterTopic = "twitter";
    private readonly string _telegramTopic = "Telegram";
    private readonly string _gptTopic = "GPT";
    private readonly ChatConfigOptions _chatConfigOptions;

    public MockDaprProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _chatConfigOptions = serviceProvider.GetRequiredService<IOptionsSnapshot<ChatConfigOptions>>().Value;
    }
    public async Task PublishEventAsync<T>(string pubsubName, string topicName, T message)
    {
       
    }
    
   
}