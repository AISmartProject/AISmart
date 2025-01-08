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
public class ConclusionGAgent : MicroAIGAgent, IConclusionGAgent
{
    private int _voteCount = 7;

    public ConclusionGAgent(ILogger<ConclusionGAgent> logger) : base(logger)
    {
    }

    [EventHandler]
    public  async Task<ConclusionGEventResponse> HandleEventAsync(ConclusionGEvent @event)
    {
        // _logger.LogInformation("ConclusionAgent " + @event.MessageId);
        List<AIMessageGEvent> list = new List<AIMessageGEvent>();
        list.Add(new AIReceiveMessageGEvent
        {
            Message = new MicroAIMessage("user", @event.Message)
        });
        
        RaiseEvents(list);
        await ConfirmEvents();
        
        ConclusionGEventResponse aiResponseEvent = new ConclusionGEventResponse();
        if (State.RecentMessages.Count >= _voteCount)
        {
            var history = State.RecentMessages.ToList();
            
            RaiseEvent(new AIClearMessageGEvent());
            await ConfirmEvents();
            
            var message = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
                .SendAsync(@event.Message, history);
            

            if (message != null && !message.Content.IsNullOrEmpty())
            {
                _logger.LogInformation("micro AI replyMessage:" + message.Content);
                await PublishAsync(new SendMessageEvent
                {
                    Message = $"this is the conclusion:{message.Content}"
                });
            }
        }

        return aiResponseEvent;
    }

    public Task SetVoteCount(int voteCount)
    {
        _voteCount = voteCount;
        return Task.CompletedTask;
    }
}