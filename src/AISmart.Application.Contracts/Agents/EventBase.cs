using System;
using System.Collections.Generic;
using Orleans;
using Orleans.Runtime;

namespace AISmart.Agents;

[GenerateSerializer]
public abstract class EventBase
{
    public Guid? CorrelationId { get; set; }
    public StreamId? StreamId { get; set; }
}