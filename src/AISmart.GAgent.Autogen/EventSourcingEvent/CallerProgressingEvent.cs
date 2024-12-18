namespace AISmart.GAgent.Autogen.EventSourcingEvent;

[GenerateSerializer]
public class CallerProgressingEvent:BaseEvent
{
    [Id(0)] public string CurrentCallInfo { get; set; }
}