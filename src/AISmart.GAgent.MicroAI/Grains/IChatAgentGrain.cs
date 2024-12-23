using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using Orleans;

namespace AISmart.Grains;

public interface IChatAgentGrain : IGrainWithStringKey
{
    Task<MicroAIMessage?> SendAsync(string message, List<MicroAIMessage>? chatHistory);
    Task SetAgentAsync(string systemMessage);
}