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

    public async Task RegisterAsync(IGAgent gAgent)
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
        return await GetGroupSubscribedEventListEvent();
    }

    private async Task<SubscribedEventListEvent> GetGroupSubscribedEventListEvent()
    {
        await LoadSubscribersAsync();

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

    [AllEventHandler]
    internal async Task ForwardEventAsync(EventWrapperBase eventWrapper)
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
        var agentGuid = this.GetPrimaryKey();
        var streamIdOfThisGAgent = StreamId.Create(CommonConstants.StreamNamespace, agentGuid);
        var streamOfThisGAgent = StreamProvider.GetStream<EventWrapperBase>(streamIdOfThisGAgent);
        if ((await streamOfThisGAgent.GetAllSubscriptionHandles()).Count == 0)
        {
            foreach (var observer in Observers.Keys)
            {
                await streamOfThisGAgent.SubscribeAsync(observer);
            }
        }

        _stateSaveTimer =
            this.RegisterGrainTimer(SaveSubscriberAsync, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        _stateSaveTimer.Dispose();
        await SaveSubscriberAsync(cancellationToken);
        await base.OnDeactivateAsync(reason, cancellationToken);
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
    
    protected sealed override async void RaiseEvent<TEvent>(TEvent @event)
    {
        base.RaiseEvent(@event);
        InternalRaiseEventAsync(@event).ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                Logger.LogError(task.Exception, "InternalRaiseEventAsync operation failed");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
    private async Task InternalRaiseEventAsync(TEvent @event)
    {
        await HandleRaiseEventAsync();
        //TODO:  need optimize use kafka,ensure Es written successfully
        await EventDispatcher.PublishAsync(@event, @event.Id.ToString());
    }
    protected virtual async Task HandleRaiseEventAsync()
    {
        
    }
}