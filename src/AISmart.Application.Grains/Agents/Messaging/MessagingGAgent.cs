using AISmart.Agents;
using AISmart.Agents.Messaging;
using AISmart.GAgent.Core;
using Microsoft.Extensions.Logging;

namespace AISmart.Application.Grains.Agents.Messaging;

public interface IMessagingGAgent : IGAgent
{
    Task<int> GetReceivedMessagesAsync();
}

public class MessagingGAgent : GAgentBase<MessagingGState, MessagingGEvent>, IMessagingGAgent
{
    public MessagingGAgent(ILogger<MessagingGAgent> logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("Agent for messaging.");
    }
    
    [EventHandler]
    public async Task OnSendEvent(SendEvent @event)
    {
        await Task.Delay(1000);
        await PublishAsync(new MessagingEvent()
        {
            Message = $"{this.GetGrainId().ToString()} sent a message."
        });
    }

    [EventHandler]
    public async Task OnMessagingEvent(MessagingEvent @event)
    {
        RaiseEvent(new MessagingGEvent());
        await ConfirmEvents();
    }

    public Task<int> GetReceivedMessagesAsync()
    {
        return Task.FromResult(State.ReceivedMessages);
    }
}