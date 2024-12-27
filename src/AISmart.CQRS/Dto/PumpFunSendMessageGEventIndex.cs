using System;
using AISmart.CQRS.Dto;
namespace AISmart.GAgent.Dto;

public class PumpFunSendMessageGEventIndex : BaseEventIndex
{
    public string? Id { get; set; }
    public string? ChatId { get; set; }
    public string? ReplyId { get; set; }
    public string? ReplyMessage { get; set; } 
}