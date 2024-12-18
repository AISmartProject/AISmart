using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using OpenAI.Chat;
using Volo.Abp.DependencyInjection;

namespace AISmart.GAgent.Autogen;

public interface IChatAgentProvider
{
    Task<IMessage> SendAsync(string agentName, string message, IEnumerable<IMessage>? chatHistory);
    MiddlewareAgent<MiddlewareStreamingAgent<OpenAIChatAgent>> GetAgent(string agentName);

    void SetAgent(string agentName, string systemMessage, FunctionCallMiddleware middleware);
}
