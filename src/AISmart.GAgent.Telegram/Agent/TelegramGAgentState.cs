using System;
using System.Collections.Generic;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using Orleans;

namespace AISmart.Agent;

public class TelegramGAgentState : StateBase
{
    [Id(0)] public Guid Id { get; set; } = Guid.NewGuid();
    
    [Id(1)] public Dictionary<string, ReceiveMessageGEvent> PendingMessages { get; set; } = new Dictionary<string, ReceiveMessageGEvent>();
    
    [Id(3)] public string BotName { get; set; } 
    
    [Id(4)] public string Token { get; set; } 
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
        BotName = setTelegramConfigEvent.BotName;
        Token = setTelegramConfigEvent.Token;
    }

}