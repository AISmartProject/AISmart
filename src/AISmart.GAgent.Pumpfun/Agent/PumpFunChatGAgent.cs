using System;
using System.Linq;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Events;
using AISmart.Grains;
using AutoGen.Core;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AISmart.Agent;

public class PumpFunChatGAgent : MicroAIGAgent, IPumpFunChatGrain
{
    private readonly string _defaultReply = "I don't understand what you're saying.";

    public PumpFunChatGAgent(ILogger<MicroAIGAgent> logger) : base(logger)
    {
    }

    [EventHandler]
    public async Task HandleEventAsync(PumpFunReceiveMessageEvent @event)
    {
        var response = string.Empty;
        try
        {
            if (!@event.RequestMessage.IsNullOrEmpty())
            {
                var message = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
                    .SendAsync(@event.RequestMessage, State.RecentMessages.ToList());
                if (message != null && !message.Content.IsNullOrWhiteSpace())
                {
                    response = message.Content;
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "[PumpFunChatGAgent] PumpFunReceiveMessageEvent error");
        }
        finally
        {
            await PublishAsync(new PumpFunSendMessageEvent() { ReplyId = @event.ReplyId, ReplyMessage = response });
            if (response != _defaultReply)
            {
                RaiseEvent(new AIReceiveMessageGEvent()
                    { Message = new MicroAIMessage(Role.User.ToString(), response) });
            }
        }
    }
}