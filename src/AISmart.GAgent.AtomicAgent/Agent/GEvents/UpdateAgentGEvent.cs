using AISmart.GAgent.AtomicAgent.Models;

namespace AISmart.GAgent.AtomicAgent.Agent.GEvents;

[GenerateSerializer]
public class UpdateAgentGEvent : AtomicAgentGEvent
{
    [Id(0)] public AgentData Data { get; set; }
}