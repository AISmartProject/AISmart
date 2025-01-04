using AISmart.Agents.GAgentBase;
using Microsoft.Extensions.Logging;

namespace AISmart.GAgent.Core;

public abstract partial class GAgentBase<TState, TEvent>
{
    private Task AddSubscriberAsync(GrainId grainId)
    {
        if (State.Subscribers.Contains(grainId))
        {
            Logger.LogError($"Cannot add duplicate subscriber {grainId}.");
            return Task.CompletedTask;
        }

        RaiseEvent((TEvent)(object)new AddSubscriberGEvent
        {
            Subscriber = grainId
        });
        ConfirmEvents();
        
        return Task.CompletedTask;
    }

    private Task RemoveSubscriberAsync(GrainId grainId)
    {
        if (!State.Subscribers.IsNullOrEmpty())
        {
            RaiseEvent((TEvent)(object)new RemoveSubscriberGEvent
            {
                Subscriber = grainId
            });
            ConfirmEvents();
        }
        
        return Task.CompletedTask;
    }

    private Task SetSubscriptionAsync(GrainId grainId)
    {
        RaiseEvent((TEvent)(object)new SetSubscriptionGEvent
        {
            Subscription = grainId
        });
        ConfirmEvents();
        return Task.CompletedTask;
    }
}