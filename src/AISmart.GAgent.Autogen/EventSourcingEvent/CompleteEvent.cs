namespace AISmart.GAgent.Autogen.EventSourcingEvent;

[GenerateSerializer]
public class CompleteEvent:BaseEvent
{
    [Id(0)] public string Summary { get; set; }
}