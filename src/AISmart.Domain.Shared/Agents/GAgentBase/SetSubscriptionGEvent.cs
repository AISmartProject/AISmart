using Orleans;
using Orleans.Runtime;

namespace AISmart.Agents.GAgentBase;

[GenerateSerializer]
public class SetSubscriptionGEvent : GEventBase
{
    [Id(0)] public GrainId Subscription { get; set; }
}