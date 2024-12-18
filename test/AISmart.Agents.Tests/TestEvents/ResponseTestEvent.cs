using AISmart.Agents;

namespace AISmart.Grains.Tests.TestEvents;

[GenerateSerializer]
public class ResponseTestEvent : EventWithResponseBase<NaiveTestEvent>
{
    [Id(0)] public string Greeting { get; set; }
}