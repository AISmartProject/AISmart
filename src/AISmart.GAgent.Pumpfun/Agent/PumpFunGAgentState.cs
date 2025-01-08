using System;
using System.Collections.Generic;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using Orleans;

namespace AISmart.Agent;
[GenerateSerializer]
public class PumpFunGAgentState : StateBase
{
    [Id(0)] private Dictionary<string, PumpFunReceiveMessageGEvent> requestMessage { get; set; } = new Dictionary<string, PumpFunReceiveMessageGEvent>();
    [Id(1)] private Dictionary<string, PumpFunSendMessageGEvent> responseMessage { get; set; } = new Dictionary<string, PumpFunSendMessageGEvent>();
    
    public void Apply(PumpFunReceiveMessageGEvent receiveMessageGEvent)
    {
        if (receiveMessageGEvent.ReplyId != null)
        {
            requestMessage[receiveMessageGEvent.ReplyId] = receiveMessageGEvent;
        }
    }
    
    public void Apply(PumpFunSendMessageGEvent sendMessageGEvent)
    {
        if (sendMessageGEvent.ReplyId != null)
        {
            responseMessage[sendMessageGEvent.ReplyId] = sendMessageGEvent;
            requestMessage.Remove(sendMessageGEvent.ReplyId);
        }
    }

}