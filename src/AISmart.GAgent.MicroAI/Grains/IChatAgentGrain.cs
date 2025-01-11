using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using Orleans;

namespace AISmart.Grains;

public interface IChatAgentGrain : IGrainWithStringKey
{
    Task<MicroAIMessage?> SendAsync(string message, List<MicroAIMessage>? chatHistory);
    Task SendEventAsync(string message, List<MicroAIMessage>? chatHistory,object requestEvent);
    Task SetAgentAsync(string systemMessage);
    Task SetAgentWithRandomLLMAsync(string systemMessage);
    Task SetAgentAsync(string systemMessage,string llm);

    Task SetAgentWithTemperature(string systemMessage, float temperature, int? seed = null,
        int? maxTokens = null);
}