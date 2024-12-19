namespace AISmart.GAgent.Autogen.EventSourcingEvent;

[GenerateSerializer]
public class BaseEvent:Agents.GEventBase
{
    [Id(0)] public Guid Id { get; set; }
}