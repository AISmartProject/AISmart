using System;
using AISmart.Agents;
using Orleans;

namespace AISmart.Agent.GEvents;
[GenerateSerializer]
public class PumpFunReceiveMessageGEvent : PumpFunMessageGEvent
{
    [Id(0)]  public string? ChatId { get; set; }
    [Id(1)]  public string? ReplyId { get; set; }
    [Id(2)] public string? RequestMessage { get; set; }
}