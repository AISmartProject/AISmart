using Orleans;
using Orleans.Runtime;

namespace AISmart.Agents.GAgentBase;

[GenerateSerializer]
public class RemoveSubscriberGEvent : GEventBase
{
    [Id(0)] public GrainId Subscriber { get; set; }
}