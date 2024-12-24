using System.Collections.Concurrent;
using AISmart.Agents;
using AISmart.Dapr;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.EventSourcing;
using Orleans.Providers;
using Orleans.Storage;
using Orleans.Streams;

namespace AISmart.GAgent.Core;

[GAgent]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public abstract partial class GAgentBase<TState, TEvent> : JournaledGrain<TState, TEvent>, IStateGAgent<TState>
    where TState : StateBase, new()
    where TEvent : GEventBase
{
    protected IStreamProvider StreamProvider => this.GetStreamProvider(CommonConstants.StreamProvider);

    protected readonly ILogger Logger;
    protected readonly IGrainStorage GrainStorage;

    /// <summary>
    /// Observer -> StreamId -> HandleId
    /// </summary>
    private readonly Dictionary<EventWrapperBaseAsyncObserver, Dictionary<StreamId, Guid>> Observers = new();

    private IEventDispatcher EventDispatcher { get; set; }

    protected GAgentBase(ILogger logger)
    {
        Logger = logger;
        GrainStorage = ServiceProvider.GetRequiredService<IGrainStorage>();
        EventDispatcher = ServiceProvider.GetRequiredService<IEventDispatcher>();
    }

    public Task ActivateAsync()
    {
        //do nothing
        return Task.CompletedTask;
    }

    public async Task<bool> SubscribeToAsync(IGAgent gAgent)
    {
        var agentGuid = gAgent.GetPrimaryKey();
        var streamId = StreamId.Create(CommonConstants.StreamNamespace, agentGuid);
        var stream = StreamProvider.GetStream<EventWrapperBase>(streamId);
        if (!await AddSubscriptionsAsync(agentGuid, stream))
        {
            return false;
        }

        await SubscribeAsync(stream);
        return true;
    }

    public async Task<bool> UnsubscribeFromAsync(IGAgent gAgent)
    {
        var agentGuid = gAgent.GetPrimaryKey();
        if (await RemoveSubscriptionsAsync(agentGuid))
        {
            var streamId = StreamId.Create(CommonConstants.StreamNamespace, agentGuid);
            var stream = StreamProvider.GetStream<EventWrapperBase>(streamId);
            var handlers = await stream.GetAllSubscriptionHandles();
            var streamHandlerIds = Observers.Select(o =>
            {
                o.Value.TryGetValue(streamId, out var handleId);
                return handleId;
            }).ToList();

            foreach (var handle in handlers.Where(h => streamHandlerIds.Contains(h.HandleId)))
            {
                await handle.UnsubscribeAsync();
            }

            return true;
        }

        return false;
    }

    public async Task<bool> PublishToAsync(IGAgent gAgent)
    {
        var agentGuid = gAgent.GetPrimaryKey();
        var streamId = StreamId.Create(CommonConstants.StreamNamespace, agentGuid);
        var stream = StreamProvider.GetStream<EventWrapperBase>(streamId);
        return await AddPublishersAsync(agentGuid, stream);
    }

    public async Task<bool> UnpublishFromAsync(IGAgent gAgent)
    {
        return await RemovePublishersAsync(gAgent.GetPrimaryKey());
    }

    public async Task RegisterAsync(IGAgent gAgent)
    {
        var success = await gAgent.SubscribeToAsync(this);
        success = await gAgent.PublishToAsync(this) | success;

        if (!success)
        {
            return;
        }

        var guid = gAgent.GetPrimaryKey();

        await AddSubscriberAsync(gAgent.GetGrainId());

        await OnRegisterAgentAsync(guid);
    }

    public async Task UnregisterAsync(IGAgent gAgent)
    {
        var success = await gAgent.UnsubscribeFromAsync(this);
        success = await gAgent.UnpublishFromAsync(this) | success;

        if (!success)
        {
            return;
        }

        await RemoveSubscriberAsync(gAgent.GetGrainId());

        await OnUnregisterAgentAsync(gAgent.GetPrimaryKey());
    }

    public async Task<List<Type>?> GetAllSubscribedEventsAsync(bool includeBaseHandlers = false)
    {
        var eventHandlerMethods = GetEventHandlerMethods();
        var handlingTypes = eventHandlerMethods
            .Select(m => m.GetParameters().First().ParameterType);
        if (!includeBaseHandlers)
        {
            handlingTypes = handlingTypes.Where(t => t != typeof(RequestAllSubscriptionsEvent));
        }

        return handlingTypes.ToList();
    }

    [EventHandler]
    public async Task<SubscribedEventListEvent> HandleRequestAllSubscriptionsEventAsync(
        RequestAllSubscriptionsEvent request)
    {
        await LoadSubscribersAsync();

        if (_subscribers.State.IsNullOrEmpty())
        {
            return new SubscribedEventListEvent
            {
                GAgentType = GetType()
            };
        }

        var gAgentList = _subscribers.State.Select(grainId => GrainFactory.GetGrain<IGAgent>(grainId)).ToList();

        if (gAgentList.IsNullOrEmpty())
        {
            return new SubscribedEventListEvent
            {
                GAgentType = GetType()
            };
        }

        if (gAgentList.Any(grain => grain == null))
        {
            // Only happened on test environment.
            throw new InvalidOperationException("One or more grains in gAgentList are null.");
        }

        var dict = new ConcurrentDictionary<Type, List<Type>>();
        foreach (var gAgent in gAgentList.AsParallel())
        {
            var eventList = await gAgent.GetAllSubscribedEventsAsync();
            dict[gAgent.GetType()] = eventList ?? [];
        }

        return new SubscribedEventListEvent
        {
            Value = dict.ToDictionary(),
            GAgentType = GetType()
        };
    }

    protected virtual async Task OnRegisterAgentAsync(Guid agentGuid)
    {
    }

    protected virtual async Task OnUnregisterAgentAsync(Guid agentGuid)
    {
    }

    public abstract Task<string> GetDescriptionAsync();

    public Task<TState> GetStateAsync()
    {
        return Task.FromResult(State);
    }

    protected async Task<Guid> PublishAsync<T>(T @event) where T : EventBase
    {
        var eventId = Guid.NewGuid();
        var eventWrapper = new EventWrapper<T>(@event, eventId, this.GetGrainId(), GetContextStorageGrainId());

        await PublishAsync(eventWrapper);

        return eventId;
    }

    private async Task PublishAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        await LoadPublishersAsync();

        if (_publishers.State.Count == 0)
        {
            return;
        }

        var eventContext = eventWrapper.Event.GetContext();
        if (!eventContext.IsNullOrEmpty())
        {
            var contextStorageGrain = eventWrapper.ContextStorageGrainId == null
                ? GrainFactory.GetGrain<IContextStorageGrain>(Guid.NewGuid())
                : GrainFactory.GetGrain<IContextStorageGrain>(eventWrapper.ContextStorageGrainId.Value.GetGuidKey());
            eventWrapper.ContextStorageGrainId = contextStorageGrain.GetGrainId();
            await contextStorageGrain.AddContext(eventContext);
        }

        foreach (var publisher in _publishers.State.Select(kp => kp.Value))
        {
            var stream = GetStream(publisher);
            await stream.OnNextAsync(eventWrapper);
        }
    }

    private async Task SubscribeAsync(IAsyncStream<EventWrapperBase> stream)
    {
        var streamId = stream.StreamId;
        foreach (var observer in Observers.Keys)
        {
            var handle = await stream.SubscribeAsync(observer);
            var handleId = handle.HandleId;
            Observers[observer][streamId] = handleId;
        }
    }

    public sealed override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await BaseOnActivateAsync(cancellationToken);
        await OnGAgentActivateAsync(cancellationToken);
    }

    protected virtual async Task OnGAgentActivateAsync(CancellationToken cancellationToken)
    {
        // Derived classes can override this method.
    }

    private async Task BaseOnActivateAsync(CancellationToken cancellationToken)
    {
        // This must be called first to initialize Observers field.
        await UpdateObserverList();

        // Register to itself.
        var agentGuid = this.GetPrimaryKey();
        var streamId = StreamId.Create(CommonConstants.StreamNamespace, agentGuid);
        var stream = StreamProvider.GetStream<EventWrapperBase>(streamId);
        foreach (var observer in Observers.Keys)
        {
            await stream.SubscribeAsync(observer);
        }

        await AddPublishersAsync(agentGuid, stream);
    }

    protected virtual async Task HandleStateChangedAsync()
    {
    }

    protected sealed override void OnStateChanged()
    {
        InternalOnStateChangedAsync().ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                Logger.LogError(task.Exception, "InternalOnStateChangedAsync operation failed");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
    private async Task InternalOnStateChangedAsync()
    {
        await HandleStateChangedAsync();
        //TODO:  need optimize use kafka,ensure Es written successfully
        await EventDispatcher.PublishAsync(State, this.GetGrainId().ToString());
    }
}