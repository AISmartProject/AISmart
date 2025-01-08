using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.SemanticKernel;

namespace AISmart.Provider;

public interface IAIAgentProvider
{
    Task<MicroAIMessage?> SendAsync(MiddlewareStreamingAgent<SemanticKernelAgent> agent,string message, List<MicroAIMessage>? chatHistory);
    Task<MiddlewareStreamingAgent<SemanticKernelAgent>> GetAgentAsync(string agentName, string systemMessage);
}
