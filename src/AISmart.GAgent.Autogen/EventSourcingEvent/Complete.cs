namespace AISmart.GAgent.Autogen.EventSourcingEvent;

[GenerateSerializer]
public class Complete:AutogenEventBase
{
    [Id(0)] public string Summary { get; set; }
}