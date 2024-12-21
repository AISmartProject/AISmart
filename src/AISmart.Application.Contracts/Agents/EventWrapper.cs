using System;
using Orleans;

namespace AISmart.Agents;

[GenerateSerializer]
public class EventWrapper<T> : EventWrapperBase
{
    // Properties with getters and setters
    [Id(0)] public T Event { get; private set; }
    [Id(1)] public Guid EventId { get; private set; }
    [Id(2)] public Guid GrainId { get; private set; }

    // Constructor
    public EventWrapper(T @event, Guid eventId, Guid grainId)
    {
        Event = @event;
        EventId = eventId;
        GrainId = grainId;
    }

    // Optionally, you can add methods or other functionality as needed
}