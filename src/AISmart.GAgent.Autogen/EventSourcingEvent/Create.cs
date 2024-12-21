using AISmart.GAgent.Autogen.Common;
using AutoGen.Core;

namespace AISmart.GAgent.Autogen.EventSourcingEvent;

[GenerateSerializer]
public class Create:AutogenEventBase
{
    [Id(0)] public List<AutogenMessage> Messages { get; set; } = new List<AutogenMessage>();
    [Id(1)] public long CreateTime { get; set; }
}