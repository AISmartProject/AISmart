using AISmart.Agents;

namespace AISmart.GAgents.Tests.TestEvents;

[GenerateSerializer]
public class ResponseTestEvent : EventWithResponseBase<NaiveTestEvent>
{
    [Id(0)] public string Greeting { get; set; }
}

[GenerateSerializer]
public class AnotherResponseTestEvent : EventWithResponseBase<NaiveTestEvent>
{
    [Id(0)] public string Greeting { get; set; }
}