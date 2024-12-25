using Orleans;

namespace AISmart.Agents.MockA.Events;

[GenerateSerializer]
public class MockAThreadCreatedEvent : EventBase
{
    [Id(0)] public string Id { get; set; }
    [Id(1)] public string Content { get; set; }
}