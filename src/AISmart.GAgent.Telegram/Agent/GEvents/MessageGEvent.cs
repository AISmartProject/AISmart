using System;
using Orleans;

namespace AISmart.GAgent.Telegram.Agent.GEvents;

[GenerateSerializer]
public class MessageGEvent :Agents.GEventBase
{
    [Id(0)] public Guid Id { get; set; } = Guid.NewGuid();
}