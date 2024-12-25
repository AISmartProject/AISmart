using AISmart.GAgent.Core;
using AISmart.GAgents.Tests.TestEvents;
using Microsoft.Extensions.Logging;

namespace AISmart.GAgents.Tests.TestGAgents;

[GAgent]
public class RelayTestGAgent : GAgentBase<NaiveTestGAgentState, NaiveTestGEvent>
{
    public RelayTestGAgent(ILogger logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("This is a GAgent used to test relay of events.");
    }

    public async Task HandleEventAsync(SocialTestEvent eventData)
    {
        if (eventData.TryGetContext("ChatId", out var chatId)
            && chatId != null)
        {
            await PublishAsync(new SendMessageTestEvent
            {
                ChatId = (string)chatId,
                Message = "I handled a social event: " + eventData.Message
            });
        }
    }
}