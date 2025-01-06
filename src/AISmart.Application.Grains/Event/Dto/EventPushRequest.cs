namespace AISmart.Application.Grains.Event.Dto;

public class EventPushRequest
{
    public string AgentId { get; set; }
    public string EventId { get; set; }
    public string EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public string Payload { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}