using System.ComponentModel;
using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Events;
using AISmart.GEvents.MicroAI;
using AISmart.Grains;
using Microsoft.Extensions.Logging;
using Orleans.Providers;


namespace AiSmart.GAgent.TestAgent.ConclusionAgent;

[Description(
    "I'm a  Summarizer, When I collect 7 votes, I will summarize the 7 votes and then send the information to Telegram.")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class ConclusionGAgent : MicroAIGAgent<ConclusionGEvent, ConclusionGEventResponse>, IConclusionGAgent
{
    private int _voteCount = 7;

    public ConclusionGAgent(ILogger<ConclusionGAgent> logger) : base(logger)
    {
    }

    [EventHandler]
    public override async Task<ConclusionGEventResponse> HandleEventAsync(ConclusionGEvent @event)
    {
        // _logger.LogInformation("ConclusionAgent " + @event.MessageId);
        List<AIMessageGEvent> list = new List<AIMessageGEvent>();
        list.Add(new AIReceiveMessageGEvent
        {
            Message = new MicroAIMessage("user", @event.Message)
        });
        
        RaiseEvent(new AIReplyMessageGEvent()
        {
            Message = new MicroAIMessage("user", @event.Message)
        });
        await ConfirmEvents();

        ConclusionGEventResponse aiResponseEvent = new ConclusionGEventResponse();
        if (State.RecentMessages.Count >= _voteCount)
        {
            RaiseEvent(new AIClearMessageGEvent());
            await ConfirmEvents();

            var message = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
                .SendAsync(@event.Message, State.RecentMessages.ToList());
            if (message != null && !message.Content.IsNullOrEmpty())
            {
                _logger.LogInformation("micro AI replyMessage:" + message.Content);
                await PublishAsync(new SendMessageEvent
                {
                    Message = $"this is the conclusion:{message.Content}"
                });
            }
        }

        RaiseEvents(list);
        await ConfirmEvents();
        return aiResponseEvent;
    }

    public Task SetVoteCount(int voteCount)
    {
        _voteCount = voteCount;
        return Task.CompletedTask;
    }
}