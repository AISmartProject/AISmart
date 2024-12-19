using System;
using AISmart.Agents;
using Orleans;

namespace AISmart.Agent.GEvents;
[GenerateSerializer]
public class TransactionGEvent : GEventBase
{
    [Id(0)] public Guid Id { get; set; } = Guid.NewGuid();
}