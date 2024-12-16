using AutoGen.Core;

namespace AISmart.GAgent.Autogen.EventSourcingEvent;

[GenerateSerializer]
public class CallerAgentReply:AutogenEventBase
{
    [Id(0)] public string AgentName { get; set; }
    [Id(1)] public IMessage Reply { get; set; }
}