using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using Orleans;

namespace AISmart.Grains;

public interface IChatAgentGrain : IGrainWithStringKey
{
    Task<MicroAIMessage?> SendAsync(string message, List<MicroAIMessage>? chatHistory);
    Task Send(string message, List<MicroAIMessage>? chatHistory);
    Task SetAgentAsync(string systemMessage);

    Task SetAgentWithTemperature(string systemMessage, float temperature, int? seed = null,
        int? maxTokens = null);
}