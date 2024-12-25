using System;
using Orleans;
using Orleans.Runtime;

namespace AISmart.Agents;

[GenerateSerializer]
public class EventWrapper<T> : EventWrapperBase
{
    // Properties with getters and setters
    [Id(0)] public T Event { get; private set; }
    [Id(1)] public Guid EventId { get; private set; }
    [Id(2)] public GrainId GrainId { get; private set; }
    [Id(3)] public GrainId? ContextStorageGrainId { get; set; } 

    // Constructor
    public EventWrapper(T @event, Guid eventId, GrainId grainId, GrainId? contextStorageGrainId)
    {
        Event = @event;
        EventId = eventId;
        GrainId = grainId;
        ContextStorageGrainId = contextStorageGrainId;

    }

    // Optionally, you can add methods or other functionality as needed
}