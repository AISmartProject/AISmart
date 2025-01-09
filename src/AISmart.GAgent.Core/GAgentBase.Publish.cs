using AISmart.Agents;
using AISmart.Dapr;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AISmart.GAgent.Core;

public abstract partial class GAgentBase<TState, TEvent>
{
    private Guid? _correlationId;

    protected async Task PublishAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        await SendEventUpwardsAsync(eventWrapper);
        await SendEventDownwardsAsync(eventWrapper);
    }

    protected async Task<Guid> PublishAsync<T>(T @event) where T : EventBase
    {
        _correlationId ??= Guid.NewGuid();
        @event.CorrelationId = _correlationId;
        Logger.LogInformation($"Published event {@event}, {_correlationId}");
        ;
        var eventId = Guid.NewGuid();
        if (State.Subscription.IsDefault)
        {
            Logger.LogInformation(
                $"Event {@event} is the first time appeared to silo: {JsonConvert.SerializeObject(@event)}");
            // This event is the first time appeared to silo.
            await SendEventToSelfAsync(new EventWrapper<T>(@event, eventId, this.GetGrainId()));
        }
        else
        {
            Logger.LogInformation(
                $"{this.GetGrainId().ToString()} is publishing event upwards: {JsonConvert.SerializeObject(@event)}");
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
        var stream = GetStream(State.Subscription.ToString());
        await stream.OnNextAsync(eventWrapper);
    }

    private async Task SendEventToSelfAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        try
        {
            Logger.LogInformation(
                $"{this.GetGrainId().ToString()} is sending event to self: {JsonConvert.SerializeObject(eventWrapper)}");
            var streamOfThisGAgent = GetStream(this.GetGrainId().ToString());
            var handles = await streamOfThisGAgent.GetAllSubscriptionHandles();
            foreach (var handle in handles)
            {
                await handle.UnsubscribeAsync();
            }

            Logger.LogInformation($"{this.GetGrainId().ToString()} has {Observers.Count} event handlers.");

            foreach (var observer in Observers.Keys)
            {
                await streamOfThisGAgent.SubscribeAsync(observer);
            }

            await streamOfThisGAgent.OnNextAsync(eventWrapper);
        }
        catch (Exception e)
        {
            Logger.LogError($"Error sending event to self: {e.Message}");
            throw;
        }
    }

    private async Task SendEventDownwardsAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        try
        {
            if (State.Subscribers.IsNullOrEmpty())
            {
                return;
            }

            Logger.LogInformation($"{this.GetGrainId().ToString()} has {State.Subscribers.Count} subscribers.");

            foreach (var grainId in State.Subscribers)
            {
                var gAgent = GrainFactory.GetGrain<IGAgent>(grainId);
                await gAgent.ActivateAsync();
                var stream = GetStream(grainId.ToString());
                var handles = await stream.GetAllSubscriptionHandles();
                Logger.LogInformation(
                    $"[SendEventDownwardsAsync]{gAgent.GetGrainId().ToString()} has {handles.Count} event handlers.");
                await stream.OnNextAsync(eventWrapper);
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"Error sending event downwards: {e.Message}");
            throw;
        }
    }
}