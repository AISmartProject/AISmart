using AISmart.GAgent.AtomicAgent.Models;

namespace AISmart.GAgent.AtomicAgent.Agent.GEvents;

[GenerateSerializer]
public class CreateAgentGEvent : AtomicAgentGEvent
{
    [Id(0)] public Guid Id { get; set; }
    [Id(1)] public string UserAddress { get; set; }
    [Id(2)] public AgentData Data { get; set; }
}