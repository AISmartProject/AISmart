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

    protected async Task<Guid> PublishAsync<T>(T @event, StreamId? streamId = null) where T : EventBase
    {
        _correlationId ??= Guid.NewGuid();
        @event.CorrelationId = _correlationId;
        streamId ??= StreamId.Create(CommonConstants.StreamNamespace, this.GetPrimaryKey());
        @event.StreamId = streamId;
        var eventId = Guid.NewGuid();
        await PublishEventDownwardsAsync(@event, eventId);
        await PublishEventUpwardsAsync(@event, eventId);
        return eventId;
    }

    private async Task PublishEventUpwardsAsync<T>(T @event, Guid eventId) where T : EventBase
    {
        if (_streamIdDictionary.TryGetValue(_correlationId!.Value, out var streamIdValue))
        {
            @event.StreamId = streamIdValue;
        }
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