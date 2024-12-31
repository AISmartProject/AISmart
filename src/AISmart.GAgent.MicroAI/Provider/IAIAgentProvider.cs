using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AutoGen.Core;
using AutoGen.OpenAI;

namespace AISmart.Provider;

public interface IAIAgentProvider
{
    Task<MicroAIMessage?> SendAsync(MiddlewareStreamingAgent<OpenAIChatAgent> agent,string message, List<MicroAIMessage>? chatHistory);
    Task<MiddlewareStreamingAgent<OpenAIChatAgent>> GetAgentAsync(string agentName, string systemMessage);
}
