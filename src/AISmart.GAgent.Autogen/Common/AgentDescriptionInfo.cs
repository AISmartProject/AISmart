using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace AISmart.GAgent.Autogen.Common;

public class AgentDescriptionInfo
{
    public string AgentName { get; set; }
    public string AgentDescription { get; set; }

    public ConcurrentBag<AgentEventDescription> EventList = new ConcurrentBag<AgentEventDescription>();
}

public class AgentEventDescription
{
    public string EventName { get; set; }
    
    [JsonIgnore]
    public Type EventType { get; set; }
    
    public string EventDescription { get; set; }

    public List<AgentEventTypeFieldDescription> EventParameters { get; set; } =
        new List<AgentEventTypeFieldDescription>();
}

public class AgentEventTypeFieldDescription
{
    public string FieldName { get; set; }
    public string FieldDescription { get; set; }
    public string FieldType { get; set; }
}