using AISmart.Agents;
using AISmart.Agents.GAgentBase;
using Microsoft.Extensions.Logging;

namespace AISmart.GAgent.Core;

public abstract partial class GAgentBase<TState, TEvent>
{
    private async Task AddSubscriberAsync(GrainId grainId)
    {
        if (State.Subscribers.Contains(grainId))
        {
            Logger.LogError($"Cannot add duplicate subscriber {grainId}.");
            return;
        }

        base.RaiseEvent(new AddSubscriberGEvent
        {
            Subscriber = grainId
        });
        await ConfirmEvents();
    }  

    private async Task RemoveSubscriberAsync(GrainId grainId)
    {
        if (!State.Subscribers.IsNullOrEmpty())
        {
            base.RaiseEvent(new RemoveSubscriberGEvent
            {
                Subscriber = grainId
            });
            await ConfirmEvents();
        }
    }

    private async Task SetSubscriptionAsync(GrainId grainId)
    {
        base.RaiseEvent(new SetSubscriptionGEvent
        {
            Subscription = grainId
        });
        await ConfirmEvents();
    }
}