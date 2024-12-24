using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AiSmart.GAgent.SocialAgent.GAgent;
using AISmart.GEvents.Social;
using AISmart.Grains;
using Json.Schema.Generation;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AiSmart.GAgent.TestAgent;

[Description("I can chat with users.")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class SocialGAgent : MicroAIGAgent, ISocialGAgent
{
    public SocialGAgent(ILogger<MicroAIGAgent> logger) : base(logger)
    {
    }

    [EventHandler]
    public  async Task<SocialResponseEvent> HandleEventAsync(SocialEvent @event)
    {
        List<AIMessageGEvent> list = new List<AIMessageGEvent>();
        list.Add(new AIReceiveMessageGEvent
        {
            Message = new MicroAIMessage("user", @event.Content)
        });

        SocialResponseEvent aiResponseEvent = new SocialResponseEvent();
        var message = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
            .SendAsync(@event.Content, State.RecentMessages.ToList());
        if (message != null && !message.Content.IsNullOrEmpty())
        {
            _logger.LogInformation(" AI replyMessage:" + message.Content);
            list.Add(new AIReplyMessageGEvent()
            {
                Message = message
            });

            aiResponseEvent.ResponseContent = message.Content;
            aiResponseEvent.ChatId = @event.ChatId;
            aiResponseEvent.ReplyMessageId = @event.MessageId;
        }

        base.RaiseEvents(list);
        return aiResponseEvent;
    }
}