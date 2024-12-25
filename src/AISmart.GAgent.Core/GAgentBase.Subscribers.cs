using AISmart.Agents;
using Orleans.Streams;

namespace AISmart.GAgent.Core;

public abstract partial class GAgentBase<TState, TEvent>
{
    protected readonly IGrainState<List<GrainId>> _subscribers = new GrainState<List<GrainId>>();

    private readonly IGrainState<Dictionary<Guid, StreamIdentity>> _subscriptions =
        new GrainState<Dictionary<Guid, StreamIdentity>>();

    private readonly IGrainState<Dictionary<Guid, StreamIdentity>> _publishers =
        new GrainState<Dictionary<Guid, StreamIdentity>>();

    protected async Task LoadSubscribersAsync()
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
        await GrainStorage.WriteStateAsync(AISmartGAgentConstants.SubscribersStateName, this.GetGrainId(),
            _subscribers);
    }

    private async Task RemoveSubscriberAsync(GrainId grainId)
    {
        await LoadSubscribersAsync();
        if (_subscribers.State.IsNullOrEmpty())
        {
            return;
        }

        _subscribers.State.Remove(grainId);
        await GrainStorage.WriteStateAsync(AISmartGAgentConstants.SubscribersStateName, this.GetGrainId(),
            _subscribers);
    }

    private async Task LoadSubscriptionsAsync()
    {
        await LoadStateAsync(_subscriptions, AISmartGAgentConstants.SubscriptionsStateName);
    }

    private async Task<bool> AddSubscriptionsAsync(Guid streamGuid, IAsyncStream<EventWrapperBase> stream)
    {
        return await AddStreamAsync(streamGuid, stream, _subscriptions, AISmartGAgentConstants.SubscriptionsStateName);
    }

    private async Task<bool> RemoveSubscriptionsAsync(Guid streamGuid)
    {
        return await RemoveStreamAsync(streamGuid, _subscriptions, AISmartGAgentConstants.SubscriptionsStateName);
    }

    private async Task LoadPublishersAsync()
    {
        await LoadStateAsync(_publishers, AISmartGAgentConstants.PublishersStateName);
    }

    private async Task<bool> AddPublishersAsync(Guid streamGuid, IAsyncStream<EventWrapperBase> stream)
    {
        return await AddStreamAsync(streamGuid, stream, _publishers, AISmartGAgentConstants.PublishersStateName);
    }

    private async Task<bool> RemovePublishersAsync(Guid streamGuid)
    {
        return await RemoveStreamAsync(streamGuid, _publishers, AISmartGAgentConstants.PublishersStateName);
    }

    private async Task LoadStateAsync(IGrainState<Dictionary<Guid, StreamIdentity>> state, string stateName)
    {
        if (state.State.IsNullOrEmpty())
        {
            await GrainStorage.ReadStateAsync(stateName, this.GetGrainId(), state);
        }
    }

    private async Task<bool> AddStreamAsync(Guid streamGuid, IAsyncStream<EventWrapperBase> stream,
        IGrainState<Dictionary<Guid, StreamIdentity>> state, string stateName)
    {
        await LoadStateAsync(state, stateName);
        state.State ??= [];
        var streamIdentity = GetStreamIdentity(streamGuid, stream);
        var success = state.State.TryAdd(streamGuid, streamIdentity);
        await GrainStorage.WriteStateAsync(stateName, this.GetGrainId(), state);
        return success;
    }

    private async Task<bool> RemoveStreamAsync(Guid streamGuid, IGrainState<Dictionary<Guid, StreamIdentity>> state,
        string stateName)
    {
        await LoadStateAsync(state, stateName);
        if (state.State.IsNullOrEmpty())
        {
            return false;
        }

        if (!state.State.Remove(streamGuid))
        {
            return false;
        }

        await GrainStorage.WriteStateAsync(stateName, this.GetGrainId(), state);
        return true;
    }

    private StreamIdentity GetStreamIdentity(Guid streamGuid, IAsyncStream<EventWrapperBase> stream)
    {
        return new StreamIdentity(streamGuid, stream.StreamId.GetNamespace());
    }

    private IAsyncStream<EventWrapperBase> GetStream(StreamIdentity streamIdentity)
    {
        var streamId = StreamId.Create(streamIdentity.Namespace, streamIdentity.Guid);
        var stream = StreamProvider.GetStream<EventWrapperBase>(streamId);
        return stream;
    }
}