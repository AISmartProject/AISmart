using System;
using System.Collections.Generic;
using Orleans;

namespace AISmart.Agents;

[GenerateSerializer]
public class SubscribedEventListEvent : EventBase
{
    [Id(0)] public List<Type> Value { get; set; }
    [Id(1)] public Type GAgentType { get; set; }
}