using System;
using Orleans;

namespace AISmart.Agent.GEvents;

public  class TransactionSuccessGEvent : TransactionGEvent
{
    [Id(1)] public Guid CreateTransactionGEventId { get; set; }
}