using Orleans;

namespace AISmart.Agents.ImplementationAgent.Events;

[GenerateSerializer]
public class ImplementationEvent:GEvent
{
    [Id(0)] public string Content { get; set; }
}