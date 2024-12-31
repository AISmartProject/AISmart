namespace AISmart.GAgent.Core;

public abstract partial class GAgentBase<TState, TEvent>
{
    private readonly IGrainState<List<GrainId>> _subscribers = new GrainState<List<GrainId>>();
    private IDisposable _stateSaveTimer;

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
    }

    private async Task RemoveSubscriberAsync(GrainId grainId)
    {
        await LoadSubscribersAsync();
        if (_subscribers.State.IsNullOrEmpty())
        {
            return;
        }

        _subscribers.State.Remove(grainId);
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