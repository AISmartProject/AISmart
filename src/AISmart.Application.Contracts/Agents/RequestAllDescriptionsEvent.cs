using System;
using Orleans;

namespace AISmart.Agents;

[GenerateSerializer]
public class RequestAllSubscriptionsEvent : EventWithResponseBase<SubscribedEventListEvent>
{
    [Id(0)] public Type RequestFromGAgentType { get; set; }
}