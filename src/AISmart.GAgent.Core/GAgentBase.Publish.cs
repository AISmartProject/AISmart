using AISmart.Agents;
using AISmart.Dapr;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans.Streams;

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
        var eventId = Guid.NewGuid();

        Logger.LogInformation($"{this.GetGrainId().ToString()}'s parent is {State.Subscription.ToString()}.");

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
            var streamOfThisGAgent = await GetStreamAsync();
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

            foreach (var subscriber in State.Subscribers)
            {
                var gAgent = GrainFactory.GetGrain<IGAgent>(subscriber);
                await gAgent.ActivateAsync();
                var streamId = StreamId.Create(CommonConstants.StreamNamespace, subscriber.ToString());
                var stream = StreamProvider.GetStream<EventWrapperBase>(streamId);
                if ((await stream.GetAllSubscriptionHandles()).Count == 0)
                {
                    await stream.SubscribeAsync(gAgent);
                }
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