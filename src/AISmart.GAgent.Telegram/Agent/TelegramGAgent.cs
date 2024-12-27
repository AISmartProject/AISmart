using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Agents.AutoGen;
using AISmart.Application.Grains;
using AISmart.Events;
using AISmart.GEvents.NLP;
using AISmart.GAgent.Core;
using AISmart.GEvents.Social;
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

    public async Task RegisterTelegramAsync(string botName, string token)
    {
        RaiseEvent(new SetTelegramConfigEvent()
        {
            BotName = botName,
            Token = token
        });
        await ConfirmEvents();
        await GrainFactory.GetGrain<ITelegramGrain>(botName).RegisterTelegramAsync(
            State.BotName, State.Token);
    }

    public async Task UnRegisterTelegramAsync(string botName)
    {
        await GrainFactory.GetGrain<ITelegramGrain>(botName).UnRegisterTelegramAsync(
            State.BotName);
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
            ChatId = @event.ChatId,
            Message = @event.Message,
            NeedReplyBotName = State.BotName
        });
        await ConfirmEvents();
        await PublishAsync(new SocialEvent()
        {
            Content = @event.Message,
            MessageId = @event.MessageId,
            ChatId = @event.ChatId
        });
        _logger.LogDebug("Publish AutoGenCreatedEvent for Telegram Message ID: " + @event.MessageId);
    }

    [EventHandler]
    public async Task HandleEventAsync(SendMessageEvent @event)
    {
        _logger.LogDebug("Publish SendMessageEvent for Telegram Message: " + @event.Message);
        await SendMessageAsync(@event.Message,@event.ChatId,@event.ReplyMessageId);
    }
    
    [EventHandler]
    public async Task HandleEventAsync(SocialResponseEvent @event)
    {
        _logger.LogDebug("SocialResponse for Telegram Message: " + @event.ResponseContent);
        await SendMessageAsync(@event.ResponseContent,@event.ChatId,@event.ReplyMessageId);
    }

    private async Task SendMessageAsync(string message,string chatId,string? replyMessageId)
    {
        if (replyMessageId != null)
        {
            RaiseEvent(new SendMessageGEvent()
            {
                ReplyMessageId = replyMessageId,
                ChatId = chatId,
                Message = message 
            });
            await ConfirmEvents();
        }

        await GrainFactory.GetGrain<ITelegramGrain>(State.BotName).SendMessageAsync(
            State.Token, chatId, message, replyMessageId);
    }
}

public interface ITelegramGAgent : IStateGAgent<TelegramGAgentState>
{
    Task RegisterTelegramAsync( string botName,string token);
    
    Task UnRegisterTelegramAsync( string botName);
    
}