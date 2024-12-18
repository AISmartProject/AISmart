using Orleans;

namespace AISmart.Agents.ImplementationAgent.Events;

[GenerateSerializer]
public class WorkCompleteEvent : EventBase
{
    [Id(0)] public string Content { get; set; }
}