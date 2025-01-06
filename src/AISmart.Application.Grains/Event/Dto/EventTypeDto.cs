namespace AISmart.Application.Grains.Event.Dto;

public class GetEventTypesInputDto
{
    public string AgentId { get; set; }
}

public class EventTypeDto
{
    public string EventType { get; set; }
    public string Description { get; set; }
    public Dictionary<string, string> Payload { get; set; }
}