using AISmart.Application.Grains.Event.Dto;

namespace AISmart.Application.Grains.Event;

using Orleans;

public interface IEventSubscriptionGrain : IGrainWithStringKey
{
    Task<SubscriptionDto> SubscribeAsync(SubscribeEventInputDto input);
    Task UnsubscribeAsync();
    Task<SubscriptionDto> GetSubscriptionStatusAsync();
    
    Task HandleEventAsync(EventPushRequest request);
}