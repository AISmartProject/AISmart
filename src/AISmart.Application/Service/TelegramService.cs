using System;
using System.Threading.Tasks;
using AISmart.Agent;
using AISmart.Agents;
using AISmart.Agents.Group;
using AISmart.Events;
using AISmart.GAgent.Autogen;
using AISmart.Sender;
using AISmart.Telegram;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Orleans;
using Volo.Abp.Application.Services;

namespace AISmart.Service;

public class TelegramService :  ApplicationService,ITelegramService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<TelegramService> _logger;
    private static readonly Guid PublishId = Guid.NewGuid();

    public TelegramService(IClusterClient clusterClient,ILogger<TelegramService> logger)
    {
        _clusterClient = clusterClient;
        _logger = logger;
    }
    
    public async Task ReceiveMessagesAsync(TelegramUpdateDto updateMessage, StringValues token)
    {
        // To filter only messages that mention the bot, check if message.Entities.type == "mention".
        // Group message auto-reply, just add the bot as a group admin.
        _logger.LogDebug("IPublishingGAgent {PublishId}",PublishId);
        if (updateMessage.Message != null)
        {
            var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(PublishId);
            await  publishingAgent.PublishEventAsync(new ReceiveMessageEvent
            {
                MessageId = updateMessage.Message.MessageId.ToString(),
                ChatId = updateMessage.Message.Chat.Id.ToString(),
                Message = updateMessage.Message.Text,
                NeedReplyBotName = token
            });
        }
    }

    public async Task SetGroupsAsync()
    {
        var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
        var telegramAgent = _clusterClient.GetGrain<IStateGAgent<TelegramGAgentState>>(Guid.NewGuid());
        var autogenAgent=  _clusterClient.GetGrain<IAutogenGAgent>(Guid.NewGuid());
        autogenAgent.RegisterAgentEvent(typeof(TelegramGAgent), [typeof(ReceiveMessageEvent), typeof(SendMessageEvent)]);
        
        await groupAgent.Register(telegramAgent);
        await groupAgent.Register(autogenAgent);
        
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(PublishId);
        await publishingAgent.PublishTo(groupAgent);
    }
}