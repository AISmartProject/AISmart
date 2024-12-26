using System;
using AISmart.Agents;
using Orleans;

namespace AISmart.Agent;

public class TwitterGAgentState : StateBase
{
    [Id(0)] public Guid Id { get; set; } = Guid.NewGuid();
    [Id(1)] public string AccountName { get; set; }
    
    
}