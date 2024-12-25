using System.Text.Json;
using System.Text.Json.Serialization;
using AISmart.Agents;
using Orleans.Streams;

namespace AISmart.GAgent.Core;

public abstract partial class GAgentBase<TState, TEvent>
{
    private readonly IGrainState<List<GrainId>> _subscribers = new GrainState<List<GrainId>>();

    private readonly IGrainState<Dictionary<Guid, string>> _subscriptions =
        new GrainState<Dictionary<Guid, string>>();

    private readonly IGrainState<Dictionary<Guid, string>> _publishers =
        new GrainState<Dictionary<Guid, string>>();

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

    private async Task LoadStateAsync(IGrainState<Dictionary<Guid, string>> state, string stateName)
    {
        await GrainStorage.ReadStateAsync(stateName, this.GetGrainId(), state);
    }

    private async Task<bool> AddStreamAsync(Guid streamGuid, IAsyncStream<EventWrapperBase> stream,
        IGrainState<Dictionary<Guid, string>> state, string stateName)
    {
        await LoadStateAsync(state, stateName);
        state.State ??= [];
        var streamIdentity = GetStreamIdentityJson(streamGuid, stream);
        var success = state.State.TryAdd(streamGuid, streamIdentity);
        await GrainStorage.WriteStateAsync(stateName, this.GetGrainId(), state);
        return success;
    }

    private async Task<bool> RemoveStreamAsync(Guid streamGuid, IGrainState<Dictionary<Guid, string>> state,
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

    private string GetStreamIdentityJson(Guid streamGuid, IAsyncStream<EventWrapperBase> stream)
    {
        return JsonSerializer.Serialize(new StreamIdentity(streamGuid, stream.StreamId.GetNamespace()!));
    }

    private IAsyncStream<EventWrapperBase> GetStream(string streamIdentityJson)
    {
        var streamIdentity = JsonSerializer.Deserialize<StreamIdentity>(streamIdentityJson);
        var streamId = StreamId.Create(streamIdentity!.Namespace, streamIdentity.Guid);
        var stream = StreamProvider.GetStream<EventWrapperBase>(streamId);
        return stream;
    }
}

public class StreamIdentity
{
    [JsonConstructor]
    public StreamIdentity(Guid guid, string @namespace)
    {
        Guid = guid;
        Namespace = @namespace;
    }

    [JsonPropertyName("guid")] public Guid Guid { get; }

    [JsonPropertyName("namespace")] public string Namespace { get; }
}