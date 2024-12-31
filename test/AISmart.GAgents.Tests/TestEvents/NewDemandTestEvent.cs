using AISmart.Agents;

namespace AISmart.GAgents.Tests.TestEvents;

[GenerateSerializer]
public class NewDemandTestEvent : EventBase
{
    [Id(0)] public string Description { get; set; }
}