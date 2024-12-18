using Orleans;

namespace AISmart.Agents.ImplementationAgent.Events;

[GenerateSerializer]
public class DeveloperEvent : EventBase
{
    [Id(0)] public string Content { get; set; }
}