using System;
using System.ComponentModel;
using AISmart.Agents;
using Orleans;

namespace AISmart.GEvents.MicroAI;
[Description("Receive a message for AI response .")]
[GenerateSerializer]
public class AIReceiveMessageEvent :  EventWithResponseBase<AIResponseEvent>
{
    [Description("Unique identifier for the received message.")]
    [Id(0)]  public Guid MessageId { get; set; }
    [Description("Unique identifier for the chat from which the message was received.")]
    [Id(1)] public string Message { get; set; }
   
}