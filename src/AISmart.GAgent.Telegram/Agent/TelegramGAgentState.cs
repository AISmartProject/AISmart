using System;
using System.Collections.Generic;
using AISmart.Agent.GEvents;
using Orleans;

namespace AISmart.Agent;

public class TelegramGAgentState 
{
    [Id(0)]  public  Guid Id { get; set; }
    
    [Id(1)] public Dictionary<string, ReceiveMessageGEvent> PendingMessages { get; set; } = new Dictionary<string, ReceiveMessageGEvent>();
    
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

}