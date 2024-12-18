namespace AISmart.GAgent.Autogen.EventSourcingEvent;

[GenerateSerializer]
public class BreakEvent:BaseEvent
{
    [Id(0)] public string BreakReason { get; set; }    
}