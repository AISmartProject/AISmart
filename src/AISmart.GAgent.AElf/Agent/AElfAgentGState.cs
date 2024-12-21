using System;
using System.Collections.Generic;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using Orleans;

namespace AISmart.Agent;

[GenerateSerializer]
public class AElfAgentGState : StateBase
{
    [Id(0)]  public  Guid Id { get; set; }
    
    [Id(1)] public Dictionary<Guid, CreateTransactionGEvent> PendingTransactions { get; set; } = new Dictionary<Guid, CreateTransactionGEvent>();
    
    public void Apply(CreateTransactionGEvent createTransactionGEvent)
    {
        if (Id == Guid.Empty)
        {
            Id = Guid.NewGuid();
        }
        PendingTransactions[createTransactionGEvent.Id] = createTransactionGEvent;
    }
    
    public void Apply(SendTransactionGEvent sendTransactionGEvent)
    {
        PendingTransactions[sendTransactionGEvent.CreateTransactionGEventId].TransactionId =
            sendTransactionGEvent.TransactionId;
    }
    
    public void Apply(TransactionSuccessGEvent transactionSuccessGEvent)
    {
        PendingTransactions.Remove(transactionSuccessGEvent.CreateTransactionGEventId);
    }
    
    public void Apply(TransactionFailedGEvent transactionFailedGEvent)
    {
        PendingTransactions.Remove(transactionFailedGEvent.CreateTransactionGEventId);
    }
    
    
    
  
}