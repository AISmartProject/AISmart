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

    public async Task RegisterAsync(IGAgent gAgent, bool isCreateNewDag = false)
    {
        var guid = gAgent.GetPrimaryKey();
        await AddSubscriberAsync(gAgent.GetGrainId());
        await OnRegisterAgentAsync(guid);
    }

    public async Task UnregisterAsync(IGAgent gAgent)
    {
        await RemoveSubscriberAsync(gAgent.GetGrainId());
        await OnUnregisterAgentAsync(gAgent.GetPrimaryKey());
    }

    public async Task<List<Type>?> GetAllSubscribedEventsAsync(bool includeBaseHandlers = false)
    {
        var eventHandlerMethods = GetEventHandlerMethods();
        eventHandlerMethods = eventHandlerMethods.Where(m => m.Name != nameof(ForwardEventAsync));
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

        var gAgentList = _subscribers.State.Keys.Select(grainId => GrainFactory.GetGrain<IGAgent>(grainId)).ToList();

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

    [AllEventHandler]
    public async Task ForwardEventAsync(EventWrapperBase eventWrapper)
    {
        await SendEventDownwardsAsync((EventWrapper<EventBase>)eventWrapper);
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

    protected async Task<Guid> PublishEventDownwardsAsync<T>(T @event) where T : EventBase
    {
        var eventId = Guid.NewGuid();
        await SendEventDownwardsAsync(new EventWrapper<T>(@event, eventId, this.GetGrainId(),
            @event.GetOriginStreamId()));
        return eventId;
    }

    protected async Task<Guid> PublishEventFromRootAsync<T>(T @event) where T : EventBase
    {
        var eventId = Guid.NewGuid();
        await SendEventToRootAsync(new EventWrapper<T>(@event, eventId, this.GetGrainId(),
            @event.GetOriginStreamId()));
        return eventId;
    }
    
    protected async Task<Guid> PublishAsync<T>(T @event) where T : EventBase
    {
        return await PublishEventFromRootAsync(@event);
    }

    private async Task SendEventDownwardsAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        await LoadSubscribersAsync();
        if (_subscribers.State.IsNullOrEmpty())
        {
            return;
        }

        eventWrapper.OriginStreamId ??= StreamId.Create(CommonConstants.StreamNamespace, this.GetPrimaryKey());

        foreach (var (subscriber, isCreateNewDag) in _subscribers.State)
        {
            if (eventWrapper.OriginStreamId == null || isCreateNewDag)
            {
                eventWrapper.OriginStreamId = StreamId.Create(CommonConstants.StreamNamespace, this.GetPrimaryKey());
            }

            var streamId = StreamId.Create(CommonConstants.StreamNamespace, subscriber.GetGuidKey());
            var stream = StreamProvider.GetStream<EventWrapperBase>(streamId);
            await stream.OnNextAsync(eventWrapper);
        }
    }

    private async Task SendEventToRootAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        if (eventWrapper.OriginStreamId == null)
        {
            return;
        }

        var stream = StreamProvider.GetStream<EventWrapperBase>(eventWrapper.OriginStreamId.Value);
        await stream.OnNextAsync(eventWrapper);
    }

    public async Task SubscribeAsync(IAsyncStream<EventWrapperBase> stream)
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
        {
            var agentGuid = this.GetPrimaryKey();
            var streamId = StreamId.Create(CommonConstants.StreamNamespace, agentGuid);
            var stream = StreamProvider.GetStream<EventWrapperBase>(streamId);
            foreach (var observer in Observers.Keys)
            {
                await stream.SubscribeAsync(observer);
            }
        }

        await LoadSubscribersAsync();
        if (_subscribers.State != null)
        {
            foreach (var subscriber in _subscribers.State.Keys)
            {
                var gAgent = GrainFactory.GetGrain<IGAgent>(subscriber);
                var streamId = StreamId.Create(CommonConstants.StreamNamespace, subscriber.GetGuidKey());
                var stream = StreamProvider.GetStream<EventWrapperBase>(streamId);
                await gAgent.SubscribeAsync(stream);
            }
        }
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