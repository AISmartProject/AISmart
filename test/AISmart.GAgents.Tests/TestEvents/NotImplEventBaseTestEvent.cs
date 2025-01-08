namespace AISmart.GAgents.Tests.TestEvents;

[GenerateSerializer]
public class NotImplEventBaseTestEvent
{
    [Id(0)] public string Greeting { get; set; }
}