using System.ComponentModel;
using AISmart.Agents;
using Orleans;

namespace AISmart.Events;

[Description("Represents an event to send a PumpFun message within a chat.")]
[GenerateSerializer]
public class PumpFunSendMessageEvent : EventBase
{
    [Description("ReplyId is from PumpFunReceiveMessageGEvent")]
    [Id(1)]
    public string? ReplyId { get; set; }

    [Description("Text content of the ReplyMessage to be sent.")]
    [Id(2)]
    public string? ReplyMessage { get; set; }
}