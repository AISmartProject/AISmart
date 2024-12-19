using System;
using System.Collections.Generic;
using AISmart.Agent.GEvents;
using Orleans;

namespace AISmart.Agent;

public class TelegramGAgentState
{
    [Id(0)] public Guid Id { get; set; } = Guid.NewGuid();
    
    [Id(1)] public Dictionary<string, ReceiveMessageGEvent> PendingMessages { get; set; } = new Dictionary<string, ReceiveMessageGEvent>();
    
    [Id(2)] public string ChatId { get; set; }
    
    [Id(3)] public string BotName { get; set; } 
    public void Apply(ReceiveMessageGEvent receiveMessageGEvent)
    {
        PendingMessages[receiveMessageGEvent.MessageId] = receiveMessageGEvent;
    }
    
    public void Apply(SendMessageGEvent sendMessageGEvent)
    {
        if (!sendMessageGEvent.ReplyMessageId.IsNullOrEmpty())
        {
            PendingMessages.Remove(sendMessageGEvent.ReplyMessageId);
        }
    }
    
    public void Apply(SetTelegramConfigEvent setTelegramConfigEvent)
    {
        ChatId = setTelegramConfigEvent.ChatId;
        BotName = setTelegramConfigEvent.BotName;
    }

}