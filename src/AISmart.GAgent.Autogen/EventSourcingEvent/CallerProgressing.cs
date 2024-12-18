namespace AISmart.GAgent.Autogen.EventSourcingEvent;

[GenerateSerializer]
public class CallerProgressing:AutogenEventBase
{
    [Id(0)] public string CurrentCallInfo { get; set; }
}