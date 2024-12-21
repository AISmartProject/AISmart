namespace AISmart.GAgent.Autogen.EventSourcingEvent;

[GenerateSerializer]
public class AutogenEventBase:Agents.GEventBase
{
    [Id(0)] public Guid TaskId { get; set; }
}