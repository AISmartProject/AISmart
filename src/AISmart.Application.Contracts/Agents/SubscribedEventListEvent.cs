using System;
using System.Collections.Generic;
using Orleans;

namespace AISmart.Agents;

[GenerateSerializer]
public class SubscribedEventListEvent : EventBase
{
    /// <summary>
    /// Key: GAgent Type.
    /// Value: Subscribed Event Types.
    /// </summary>
    [Id(0)] public Dictionary<Type, List<Type>> Value { get; set; }
    [Id(1)] public Type GAgentType { get; set; }
}