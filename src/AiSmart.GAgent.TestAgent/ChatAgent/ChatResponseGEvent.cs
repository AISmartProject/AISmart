using AISmart.Agents;
using AISmart.GAgent.Autogen.EventSourcingEvent;

namespace AiSmart.GAgent.TestAgent;

[GenerateSerializer]
public class ChatResponseGEvent:EventBase
{
    [Id(0)] public string ResponseContent { get; set; }
}