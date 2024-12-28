using System;
using System.Collections.Generic;
using Orleans;
using Orleans.Runtime;

namespace AISmart.Agents;

[GenerateSerializer]
public class EventWrapper<T> : EventWrapperBase where T : EventBase
{
    [Id(0)] public T Event { get; private set; }
    [Id(1)] public Guid EventId { get; private set; }
    [Id(2)] public GrainId GrainId { get; private set; }
    [Id(3)] public Guid? CorrelationId { get; set; }
    [Id(4)] public StreamId? StreamId { get; set; }

    public EventWrapper(T @event, Guid eventId, GrainId grainId)
    {
        Event = @event;
        EventId = eventId;
        GrainId = grainId;
        CorrelationId = @event.CorrelationId;
        StreamId = @event.StreamId;
    }
}