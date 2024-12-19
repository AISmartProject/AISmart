using System;
using Orleans;

namespace AISmart.Agent.GEvents;

[GenerateSerializer]
public  class SendTransactionGEvent:  TransactionGEvent
{
   
    [Id(1)] public Guid CreateTransactionGEventId { get; set; }
    [Id(2)] public string ChainId { get; set; }
    [Id(3)] public bool IsSuccess   { get; set; }
    
    [Id(4)] public string TransactionId { get; set; }
}