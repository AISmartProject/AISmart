using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Agents.AutoGen;
using AISmart.Application.Grains;
using AISmart.Events;
using AISmart.GEvents.NLP;
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
        return Task.FromResult(
            "Represents an agent responsible for informing other agents when a Telegram thread is published.");
    }

    public async Task SetTelegramConfig(string chatId, string botName)
    {
        RaiseEvent(new SetTelegramConfigEvent()
        {
            ChatId = chatId,
            BotName = botName
        });
        await ConfirmEvents();
    }


    [EventHandler]
    public async Task HandleEventAsync(ReceiveMessageEvent @event)
    {
        _logger.LogInformation("Telegram ReceiveMessageEvent " + @event.MessageId);
        if (State.PendingMessages.TryGetValue(@event.MessageId, out _))
        {
            _logger.LogDebug("Message reception repeated for Telegram Message ID: " + @event.MessageId);
            return;
        }

        RaiseEvent(new ReceiveMessageGEvent
        {
            MessageId = @event.MessageId,
            ChatId = State.ChatId,
            Message = @event.Message,
            NeedReplyBotName = State.BotName
        });
        await ConfirmEvents();
//         await PublishAsync(new AutoGenCreatedEvent
//         {
//             EventId = Guid.NewGuid(),
//             Content =
//                 $"""
//                  Received a JSON-formatted message:{JsonConvert.SerializeObject(@event)}, The fields in the will be used in the final response except "Message".
//                  Please follow the process below.
//                  1. parse the message content,the fields in the JSON may be used in the final response..
//                  2. Please understand the content of the "Message" in the JSON format, process the response accordingly.
//                  """
//         });
        await PublishAsync(new NLPGEvent() { Content = @event.Message });
        _logger.LogDebug("Publish AutoGenCreatedEvent for Telegram Message ID: " + @event.MessageId);
    }

    [EventHandler]
    public async Task HandleEventAsync(SendMessageEvent @event)
    {
        _logger.LogDebug("Publish SendMessageEvent for Telegram Message: " + @event.Message);
        if (@event.ReplyMessageId != null)
        {
            RaiseEvent(new SendMessageGEvent()
            {
                ReplyMessageId = @event.ReplyMessageId,
                ChatId = State.ChatId,
                Message = @event.Message.IsNullOrEmpty() ? @event.Photo : @event.Message
            });
            await ConfirmEvents();
        }

        await GrainFactory.GetGrain<ITelegramGrain>(Guid.NewGuid()).SendMessageAsync(
            State.BotName, State.ChatId, @event.Message, @event.ReplyMessageId);
    }
}

public interface ITelegramGAgent : IStateGAgent<TelegramGAgentState>
{
    Task SetTelegramConfig(string chatId, string botName);
}