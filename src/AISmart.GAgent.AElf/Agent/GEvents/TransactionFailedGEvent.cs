using System;
using Orleans;

namespace AISmart.Agent.GEvents;
[GenerateSerializer]
public  class TransactionFailedGEvent : CreateTransactionGEvent
{
    [Id(1)] public Guid CreateTransactionGEventId { get; set; }
    [Id(2)] public string Error { get; set; }
}