using Orleans;

namespace AISmart.Agents.X.Events;

[GenerateSerializer]
public class XThreadCreatedEvent : EventBase
{
    [Id(0)] public string Id { get; set; }
    [Id(1)] public string Content { get; set; }
}