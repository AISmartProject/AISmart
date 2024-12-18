using Orleans;

namespace AISmart.Agents.ImplementationAgent.Events;

[GenerateSerializer]
public class ImplementationEvent : EventWithResponseBase<WorkCompleteEvent>
{
    [Id(0)] public string Content { get; set; }
}