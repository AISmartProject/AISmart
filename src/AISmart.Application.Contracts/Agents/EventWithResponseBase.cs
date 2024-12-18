using Orleans;

namespace AISmart.Agents;

[GenerateSerializer]
public abstract class EventWithResponseBase<TResponseEvent> : EventBase where TResponseEvent : EventBase
{
}