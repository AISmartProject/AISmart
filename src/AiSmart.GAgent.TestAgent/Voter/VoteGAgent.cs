using System.ComponentModel;
using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Events;
using AiSmart.GAgent.TestAgent.ConclusionAgent;
using AISmart.GEvents.MicroAI;
using AISmart.Grains;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AiSmart.GAgent.TestAgent.Voter;

[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class VoteGAgent : MicroAIGAgent, IVoterGAgent
{
    public VoteGAgent(ILogger<MicroAIGAgent> logger) : base(logger)
    {
    }

    [EventHandler]
    public  async Task<VoteGEventResponse> HandleEventAsync(VoterGEvent @event)
    {
        // _logger.LogInformation("Vote AI ReceiveMessageEvent " + @event.MessageId);
        List<AIMessageGEvent> list = new List<AIMessageGEvent>();
        list.Add(new AIReceiveMessageGEvent
        {
            Message = new MicroAIMessage("user", @event.Message)
        });
        
        VoteGEventResponse aiResponseEvent = new VoteGEventResponse();
        var message = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
            .SendAsync(@event.Message, State.RecentMessages.ToList());
        if (message != null && !message.Content.IsNullOrEmpty())
        {
            _logger.LogInformation("AI AI replyMessage:" + message.Content);
            list.Add(new AIReplyMessageGEvent()
            {
                Message = message
            });

            aiResponseEvent.Content = "Done";
            await PublishAsync(new ConclusionGEvent()
            {
                Message = message.Content
            });
            
            await PublishAsync(new SendMessageEvent
            {
                Message = $"{State.AgentName}:{message.Content}"
            });
        }

        RaiseEvents(list);
        await ConfirmEvents();
        return aiResponseEvent;
    }
}