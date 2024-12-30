using AISmart.Agents;
using AISmart.Dapr;

namespace AISmart.GAgent.Core;

public abstract partial class GAgentBase<TState, TEvent>
{
    private Guid? _correlationId = null;
    
    private readonly Dictionary<Guid, StreamId> _streamIdDictionary = new();

    protected async Task PublishAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        await SendEventUpwardsAsync(eventWrapper);
        await SendEventDownwardsAsync(eventWrapper);
    }

    protected async Task<Guid> PublishAsync<T>(T @event) where T : EventBase
    {
        var isTop = _correlationId == null;
        _correlationId ??= Guid.NewGuid();
        @event.CorrelationId = _correlationId;
        @event.StreamId = StreamId.Create(CommonConstants.StreamNamespace, this.GetPrimaryKey());
        var eventId = Guid.NewGuid();
        switch (isTop)
        {
            case true:
                // This event is the first time appeared to silo.
                await SendEventToSelfAsync(new EventWrapper<T>(@event, eventId, this.GetGrainId()));
                break;
            case false when _streamIdDictionary.TryGetValue(_correlationId!.Value, out var streamIdValue):
                @event.StreamId = streamIdValue;
                await PublishEventUpwardsAsync(@event, eventId);
                break;
        }

        return eventId;
    }

    private async Task PublishEventUpwardsAsync<T>(T @event, Guid eventId) where T : EventBase
    {
        await SendEventUpwardsAsync(new EventWrapper<T>(@event, eventId, this.GetGrainId()));
    }

    private async Task SendEventUpwardsAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        var stream = StreamProvider.GetStream<EventWrapperBase>(eventWrapper.StreamId!.Value);
        await stream.OnNextAsync(eventWrapper);
    }

    private async Task PublishEventDownwardsAsync<T>(T @event, Guid eventId) where T : EventBase
    {
        @event.CorrelationId ??= Guid.NewGuid();
        var streamOfThisGAgent = StreamId.Create(CommonConstants.StreamNamespace, this.GetPrimaryKey());
        @event.StreamId ??= streamOfThisGAgent;
        await SendEventDownwardsAsync(new EventWrapper<T>(@event, eventId, this.GetGrainId()));
    }
    
    private async Task SendEventToSelfAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        var streamIdOfThisGAgent = StreamId.Create(CommonConstants.StreamNamespace, this.GetPrimaryKey());
        var streamOfThisGAgent = StreamProvider.GetStream<EventWrapperBase>(streamIdOfThisGAgent);
        await streamOfThisGAgent.OnNextAsync(eventWrapper);
    }

    private async Task SendEventDownwardsAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        await LoadSubscribersAsync();
        if (_subscribers.State.IsNullOrEmpty())
        {
            return;
        }

        foreach (var stream in _subscribers.State
                     .Select(subscriber => StreamId.Create(CommonConstants.StreamNamespace, subscriber.GetGuidKey()))
                     .Select(streamId => StreamProvider.GetStream<EventWrapperBase>(streamId)))
        {
            await stream.OnNextAsync(eventWrapper);
        }
    }
}