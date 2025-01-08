using AISmart.GAgent.Autogen.Common;
using AutoGen.Core;

namespace AISmart.GAgent.Autogen.EventSourcingEvent;

[GenerateSerializer]
public class CallAgentReply:AutogenEventBase
{
    [Id(0)] public string AgentName { get; set; }
    [Id(1)] public Guid  EventId { get; set; }
    [Id(2)] public AutogenMessage Reply { get; set; }
}