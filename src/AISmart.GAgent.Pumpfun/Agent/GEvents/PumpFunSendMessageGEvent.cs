using Orleans;

namespace AISmart.Agent.GEvents;
[GenerateSerializer]
public class PumpFunSendMessageGEvent : PumpFunMessageGEvent
{
    [Id(1)] public string? ChatId { get; set; }
    [Id(2)] public string? ReplyId { get; set; }
    [Id(3)] public string? ReplyMessage { get; set; } 
}