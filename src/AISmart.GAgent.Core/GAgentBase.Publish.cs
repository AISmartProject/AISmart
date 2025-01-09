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

            var tasks = State.Subscribers.Select(async grainId =>
            {
                var gAgent = GrainFactory.GetGrain<IGAgent>(grainId);
                await gAgent.ActivateAsync();
                var stream = await gAgent.GetStreamAsync();
                var handles = await stream.GetAllSubscriptionHandles();
                var retryCount = 0;
                while (handles.Count == 0 && retryCount < 10)
                {
                    Logger.LogInformation($"Retrying to get subscription handles for {gAgent.GetGrainId().ToString()}, attempt {retryCount + 1}");
                    await Task.Delay(100); // Optional delay between retries
                    stream = await gAgent.GetStreamAsync();
                    handles = await stream.GetAllSubscriptionHandles();
                    retryCount++;
                }

                Logger.LogInformation(
                    $"Stream of {gAgent.GetGrainId().ToString()} has {handles.Count} subscription handles.");

                foreach (var handle in handles)
                {
                    Logger.LogInformation(
                        $"Stream of {gAgent.GetGrainId().ToString()}, handle {handle.HandleId}, stream id: {handle.StreamId}");
                }

                await stream.OnNextAsync(eventWrapper);
            }).ToList();

            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            Logger.LogError($"Error sending event downwards: {e.Message}");
            throw;
        }
    }
}