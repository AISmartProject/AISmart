using System;
using Orleans;

namespace AISmart.Agent.GEvents;

[GenerateSerializer]
public class PumpFunMessageGEvent :Agents.GEventBase
{
    [Id(0)] public override Guid Id { get; set; }
}