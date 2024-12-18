using System;
using Orleans;

namespace AISmart.Agent.GEvents;

[GenerateSerializer]
public class MessageGEvent :Agents.GEventBase
{
    [Id(0)] public Guid Id { get; set; }
}