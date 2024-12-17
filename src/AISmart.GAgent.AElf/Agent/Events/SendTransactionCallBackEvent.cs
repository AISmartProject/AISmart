using System;
using AISmart.Agents;
using Google.Protobuf;
using Orleans;

namespace AISmart.Agent.Event;

[GenerateSerializer]
public  class SendTransactionCallBackEvent :EventBase
{
    [Id(0)] public Guid Id { get; set; }
    
    [Id(2)] public string TransactionId { get; set; }
    [Id(3)] public string ChainId { get; set; }
    [Id(4)] public Guid CreateTransactionGEventId { get; set; }
}