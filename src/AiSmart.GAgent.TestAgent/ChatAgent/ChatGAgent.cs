using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Events;
using AiSmart.GAgent.TestAgent.ConclusionAgent;
using AISmart.Grains;
using Json.Schema.Generation;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AiSmart.GAgent.TestAgent;

[Description("I can chat with users.")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class ChatGAgent : MicroAIGAgent, IChatGAgent
{
    public ChatGAgent(ILogger<MicroAIGAgent> logger) : base(logger)
    {
    }

    [EventHandler]
    public  async Task<ChatResponseGEvent> HandleEventAsync(ChatGEvent gEvent)
    {
        List<AIMessageGEvent> list = new List<AIMessageGEvent>();
        list.Add(new AIReceiveMessageGEvent
        {
            Message = new MicroAIMessage("user", gEvent.Content)
        });

        ChatResponseGEvent aiResponseEvent = new ChatResponseGEvent();
        var message = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
            .SendAsync(gEvent.Content, State.RecentMessages.ToList());
        if (message != null && !message.Content.IsNullOrEmpty())
        {
            _logger.LogInformation("AI replyMessage:" + message.Content);
            list.Add(new AIReplyMessageGEvent()
            {
                Message = message
            });

            aiResponseEvent.ResponseContent = "Done";

            // await PublishAsync(new SendMessageEvent
            // {
            //     Message = message.Content
            // });
        }

        base.RaiseEvents(list);
        return aiResponseEvent;
    }
}