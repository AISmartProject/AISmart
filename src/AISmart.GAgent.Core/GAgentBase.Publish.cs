using AISmart.Agents;
using AISmart.Dapr;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
        Logger.LogInformation($"Published event {@event}, {isTop}, {_correlationId}");;
        var eventId = Guid.NewGuid();
        await LoadSubscriptionAsync();
        if (_subscription.State.IsDefault)
        {
            Logger.LogInformation($"Event {@event} is the first time appeared to silo: {JsonConvert.SerializeObject(@event)}");
            // This event is the first time appeared to silo.
            await SendEventToSelfAsync(new EventWrapper<T>(@event, eventId, this.GetGrainId()));
        }
        else
        {
            Logger.LogInformation($"{this.GetGrainId().ToString()} is publishing event upwards: {JsonConvert.SerializeObject(@event)}");
            await PublishEventUpwardsAsync(@event, eventId);
        }

        return eventId;
    }

    private async Task PublishEventUpwardsAsync<T>(T @event, Guid eventId) where T : EventBase
    {
        await SendEventUpwardsAsync(new EventWrapper<T>(@event, eventId, this.GetGrainId()));
    }

    private async Task SendEventUpwardsAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        var stream = GetStream(_subscription.State.ToString());
        await stream.OnNextAsync(eventWrapper);
    }

    private async Task SendEventToSelfAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        Logger.LogInformation(
            $"{this.GetGrainId().ToString()} is sending event to self: {JsonConvert.SerializeObject(eventWrapper)}");
        var streamOfThisGAgent = GetStream(this.GetGrainId().ToString());
        var handles = await streamOfThisGAgent.GetAllSubscriptionHandles();
        foreach (var handle in handles)
        {
            await handle.UnsubscribeAsync();
        }

        foreach (var observer in Observers.Keys)
        {
            await streamOfThisGAgent.SubscribeAsync(observer);
        }

        await streamOfThisGAgent.OnNextAsync(eventWrapper);
    }

    private async Task SendEventDownwardsAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        await LoadSubscribersAsync();
        if (_subscribers.State.IsNullOrEmpty())
        {
            return;
        }

        Logger.LogInformation($"{this.GetGrainId().ToString()} has {_subscribers.State.Count} subscribers.");

        foreach (var grainId in _subscribers.State)
        {
            var gAgent = GrainFactory.GetGrain<IGAgent>(grainId);
            await gAgent.ActivateAsync();
            var stream = GetStream(grainId.ToString());
            await stream.OnNextAsync(eventWrapper);
        }
    }
}