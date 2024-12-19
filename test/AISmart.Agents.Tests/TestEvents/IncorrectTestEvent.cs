namespace AISmart.Grains.Tests.TestEvents;

[GenerateSerializer]
public class IncorrectTestEvent
{
    [Id(0)] public string Greeting { get; set; }
}