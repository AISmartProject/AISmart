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
    }

    protected async Task<Guid> PublishAsync<T>(T @event) where T : EventBase
    {
        _correlationId ??= Guid.NewGuid();
        @event.CorrelationId = _correlationId;
        @event.StreamId = StreamId.Create(CommonConstants.StreamNamespace, this.GetPrimaryKey());
        var eventId = Guid.NewGuid();

        await SendEventToSelfStreamAsync(new EventWrapper<T>(@event, eventId, this.GetGrainId()));
        await PublishEventUpwardsAsync(@event, eventId);

        return eventId;
    }

    private async Task PublishEventUpwardsAsync<T>(T @event, Guid eventId) where T : EventBase
    {
        await SendEventUpwardsAsync(new EventWrapper<T>(@event, eventId, this.GetGrainId()));
    }

    private async Task SendEventUpwardsAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        var stream = StreamProvider.GetStream<EventWrapperBase>(_parentStreamId.State);
        await stream.OnNextAsync(eventWrapper);
    }
    
    private async Task SendEventToSelfStreamAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        var streamOfThisGAgent = StreamProvider.GetStream<EventWrapperBase>(eventWrapper.StreamId!.Value);
        await streamOfThisGAgent.OnNextAsync(eventWrapper);
    }
}