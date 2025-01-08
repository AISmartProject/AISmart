using Orleans;

namespace AISmart.Agents.MockB.Events;

[GenerateSerializer]
public class MockBThreadCreatedEvent : EventBase
{
    [Id(0)] public string Id { get; set; }
    [Id(1)] public string Content { get; set; }
}