namespace AISmart.GAgent.Core;

public abstract partial class GAgentBase<TState, TEvent>
{
    private readonly IGrainState<List<GrainId>> _subscribers = new GrainState<List<GrainId>>();
    private readonly IGrainState<GrainId> _subscription = new GrainState<GrainId>();
    private IDisposable? _stateSaveTimer;

    private async Task LoadSubscribersAsync()
    {
        if (_subscribers.State.IsNullOrEmpty())
        {
            await GrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscribersStateName, this.GetGrainId(),
                _subscribers);
        }
    }

    private async Task AddSubscriberAsync(GrainId grainId)
    {
        await LoadSubscribersAsync();
        _subscribers.State ??= [];
        _subscribers.State.Add(grainId);
        await SaveSubscriberAsync(CancellationToken.None);
    }

    private async Task RemoveSubscriberAsync(GrainId grainId)
    {
        await LoadSubscribersAsync();
        if (_subscribers.State.IsNullOrEmpty())
        {
            return;
        }

        if (_subscribers.State.Remove(grainId))
        {
            await GrainStorage.WriteStateAsync(AISmartGAgentConstants.SubscribersStateName, this.GetGrainId(),
                _subscribers);
        }
    }

    private async Task SetSubscriptionAsync(GrainId grainId)
    {
        var storedSubscription = new GrainState<GrainId>();
        await GrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscriptionStateName, this.GetGrainId(),
            storedSubscription);
        if (!storedSubscription.State.IsDefault)
        {
            await GrainStorage.ClearStateAsync(AISmartGAgentConstants.SubscriptionStateName, this.GetGrainId(),
                storedSubscription);
        }

        _subscription.State = grainId;
        await GrainStorage.WriteStateAsync(AISmartGAgentConstants.SubscriptionStateName, this.GetGrainId(),
            _subscription);
    }

    private async Task SaveSubscriberAsync(CancellationToken cancellationToken)
    {
        if (!_subscribers.State.IsNullOrEmpty())
        {
            await GrainStorage.WriteStateAsync(AISmartGAgentConstants.SubscribersStateName, this.GetGrainId(),
                _subscribers);
        }
    }
}