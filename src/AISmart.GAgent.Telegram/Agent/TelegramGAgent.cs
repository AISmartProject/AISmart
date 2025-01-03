using System.ComponentModel;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.Events;
using AISmart.GAgent.Core;
using AISmart.GAgent.Telegram.Agent.GEvents;
using AISmart.GAgent.Telegram.Grains;
using AISmart.GEvents.Social;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Orleans.Providers;

namespace AISmart.GAgent.Telegram.Agent;

[Description("Handle telegram")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class TelegramGAgent : GAgentBase<TelegramGAgentState, MessageGEvent>, ITelegramGAgent
{
    public TelegramGAgent(ILogger<TelegramGAgent> logger) : base(logger)
    {
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
            State.Token);
    }


    [EventHandler]
    public async Task HandleEventAsync(ReceiveMessageEvent @event)
    {
        Logger.LogInformation("Telegram ReceiveMessageEvent " + @event.MessageId);
        if (!@event.MessageId.IsNullOrEmpty())
        {
            if (State.PendingMessages.TryGetValue(@event.MessageId, out _))
            {
                Logger.LogDebug("Message reception repeated for Telegram Message ID: " + @event.MessageId);
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
        }
        await PublishAsync(new SocialEvent()
        {
            Content = @event.Message,
            MessageId = @event.MessageId,
            ChatId = @event.ChatId
        });
        Logger.LogDebug("Publish AutoGenCreatedEvent for Telegram Message ID: " + @event.MessageId);
    }

    [EventHandler]
    public async Task HandleEventAsync(SendMessageEvent @event)
    {
        Logger.LogDebug("Publish SendMessageEvent for Telegram Message: " + @event.Message);
        await SendMessageAsync(@event.Message,@event.ChatId,@event.ReplyMessageId);
    }
    
    [EventHandler]
    public async Task HandleEventAsync(SocialResponseEvent @event)
    {
        Logger.LogDebug("SocialResponse for Telegram Message: " + @event.ResponseContent);
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