using System.Net.Http.Json;
using AISmart.Application.Grains.Event.Dto;

namespace AISmart.Application.Grains.Event;

using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class EventSubscriptionGrain : Grain, IEventSubscriptionGrain
{
    private SubscriptionDto _subscription; 

    
    public async Task<SubscriptionDto> SubscribeAsync(SubscribeEventInputDto input)
    {
        _subscription = new SubscriptionDto
        {
            SubscriptionId = Guid.NewGuid().ToString(),
            AgentId = this.GetPrimaryKeyString(), 
            EventTypes = input.EventTypes.Count > 0 ? input.EventTypes : new List<string> { "ALL" }, // 默认为 "ALL"
            CallbackUrl = input.CallbackUrl,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };

        return await Task.FromResult(_subscription);
    }

    public async Task UnsubscribeAsync()
    {
        if (_subscription == null)
        {
           return;
        }

        _subscription.Status = "cancelled";
        _subscription = null;
        await Task.CompletedTask;
    }

    public async Task<SubscriptionDto> GetSubscriptionStatusAsync()
    {
        if (_subscription == null)
        {
            throw new KeyNotFoundException("No active subscription found.");
        }

        return await Task.FromResult(_subscription);
    }
    
    public async Task HandleEventAsync(EventPushRequest request)
    {
        var subscriptionGrain = GrainFactory.GetGrain<IEventSubscriptionGrain>(request.AgentId);
        var subscription = await subscriptionGrain.GetSubscriptionStatusAsync();

        if (subscription != null && subscription.Status == "active")
        {
            using var httpClient = new HttpClient();
            await httpClient.PostAsJsonAsync(subscription.CallbackUrl, request);
        }
    }
}
