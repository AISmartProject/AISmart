using AISmart.GAgent.Autogen.Event;
using Orleans;

namespace AISmart.GEvents.Autogen;


[GenerateSerializer]
public class PassThroughExecutorEvent:AutoGenExecutorEvent
{
    [Id(0)]public object PassThroughData { get; set; }
}