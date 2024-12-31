using System;
using AISmart.CQRS.Dto;
namespace AISmart.GAgent.Dto;

public class CreateTransactionGEventIndex : BaseEventIndex
{
    public Guid Id { get; set; }
    public string ChainId { get; set; }
    public string SenderName{ get; set; }
    public string ContractAddress { get; set; }
    public string MethodName { get; set; }
    public string Param { get; set; }
    public bool IsSuccess   { get; set; }
    
    public string TransactionId { get; set; }
}
