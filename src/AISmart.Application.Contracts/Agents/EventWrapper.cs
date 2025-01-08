using Orleans;
using Orleans.Runtime;

namespace AISmart.Agents;

[GenerateSerializer]
public class EventWrapper<T> : EventWrapperBase
{
    // Properties with getters and setters
    [Id(0)] public T Event { get; private set; }
    [Id(1)] public GrainId GrainId { get; private set; }

    [Id(2)] public int count { get; set; } = 0;

    // Constructor
    public EventWrapper(T @event, GrainId grainId)
    {
        Event = @event;
        GrainId = grainId;
    }

    // Optionally, you can add methods or other functionality as needed
}