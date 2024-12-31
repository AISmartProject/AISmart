using AISmart.Agents;

namespace AISmart.GAgents.Tests.TestEvents;

[GenerateSerializer]
public class WorkingOnTestEvent : EventBase
{
    [Id(0)] public string Description { get; set; }
}