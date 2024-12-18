using AISmart.Agents;

namespace AISmart.Grains.Tests.TestEvents;

[GenerateSerializer]
public class NaiveTestEvent : EventBase
{
    [Id(0)] public string Greeting { get; set; }
}