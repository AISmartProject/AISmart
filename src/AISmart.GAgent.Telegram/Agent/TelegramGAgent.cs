using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Agents.AutoGen;
using AISmart.Application.Grains;
using AISmart.Events;
using AISmart.Grains;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans.Providers;

namespace AISmart.Agent;

[Description("Handle telegram")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class TelegramGAgent : GAgentBase<TelegramGAgentState, MessageGEvent>, ITelegramGAgent
{
    private readonly ILogger<TelegramGAgent> _logger;
    public TelegramGAgent(ILogger<TelegramGAgent> logger) : base(logger)
    {
        _logger = logger;
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("Represents an agent responsible for informing other agents when a Telegram thread is published.");
    }
    

    [EventHandler]
    public async Task HandleEventAsync(ReceiveMessageEvent @event)
    { 
        _logger.LogInformation("Telegram ReceiveMessageEvent "+@event.MessageId);
        if (State.PendingMessages.TryGetValue(@event.MessageId,out _))
        {
            _logger.LogDebug("Telegram Message reception repeated for Telegram Message ID: " + @event.MessageId);
            return;
        }
        RaiseEvent(new ReceiveMessageGEvent
       {
           MessageId = @event.MessageId,
           ChatId = @event.ChatId,
           Message = @event.Message,
           NeedReplyBotName = @event.NeedReplyBotName
       });
       await ConfirmEvents();
       await PublishAsync(new AutoGenCreatedEvent
       {
           EventId = Guid.NewGuid(),
           Content = $"I received a JSON-formatted message:{JsonConvert.SerializeObject(@event)}. Please parse the message content, generate a response Based on the JSON Message, and then call the SendMessageEvent event of TelegramGAgent"
       });
       _logger.LogDebug("Telegram Publish AutoGenCreatedEvent for Telegram Message ID: " + @event.MessageId);
    }
    [EventHandler]
    public async Task HandleEventAsync(SendMessageEvent @event)
    {
        _logger.LogDebug("Telegram SendMessageEvent for Telegram Message: " + @event.Message);
        var senderBotName = @event.SenderBotName;
        if (@event.ReplyMessageId != null)
        {
            if (State.PendingMessages.TryGetValue(@event.ReplyMessageId, out var receiveMessageEvent))
            {
                senderBotName = receiveMessageEvent.NeedReplyBotName;
            }
            RaiseEvent(new SendMessageGEvent()
            {
                ReplyMessageId = @event.ReplyMessageId,
                ChatId = @event.ChatId,
                Message = @event.Message.IsNullOrEmpty() ? 
                    @event.Photo : @event.Message
            });
            await ConfirmEvents();
        }
        await GrainFactory.GetGrain<ITelegramGrain>(Guid.NewGuid()).SendMessageAsync(
            senderBotName , @event.ChatId, @event.Message, @event.ReplyMessageId);
        
    }

    
}

public interface ITelegramGAgent
{ 
   
}