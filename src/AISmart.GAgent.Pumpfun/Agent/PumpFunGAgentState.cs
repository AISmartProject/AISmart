using System;
using System.Collections.Generic;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using Orleans;

namespace AISmart.Agent;
[GenerateSerializer]
public class PumpFunGAgentState : StateBase
{
    [Id(0)] public Guid Id { get; set; } = Guid.NewGuid();
    
    [Id(1)] public Dictionary<string, PumpFunReceiveMessageGEvent> requestMessage { get; set; } = new Dictionary<string, PumpFunReceiveMessageGEvent>();
    [Id(2)] public Dictionary<string, PumpFunSendMessageGEvent> responseMessage { get; set; } = new Dictionary<string, PumpFunSendMessageGEvent>();
    
    [Id(3)] public string ChatId { get; set; }
    
    public void Apply(PumpFunReceiveMessageGEvent receiveMessageGEvent)
    {
        requestMessage[receiveMessageGEvent.ReplyId] = receiveMessageGEvent;
    }
    
    public void Apply(PumpFunSendMessageGEvent sendMessageGEvent)
    {
        responseMessage[sendMessageGEvent.ReplyId] = sendMessageGEvent;
        requestMessage.Remove(sendMessageGEvent.ReplyId);
    }
    
    public void Apply(SetPumpFunConfigEvent setPumpFunConfigEvent)
    {
        ChatId = setPumpFunConfigEvent.ChatId;
    }

}