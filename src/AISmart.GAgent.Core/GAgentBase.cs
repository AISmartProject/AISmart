using System.Collections.Concurrent;
using Aevatar.Core;
using Aevatar.Core.Abstractions;
using AISmart.Agents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans.EventSourcing;
using Orleans.Providers;
using Orleans.Streams;

namespace AISmart.GAgent.Core;

[GAgent]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public abstract partial class GAgentBase<TState, TEvent> : JournaledGrain<TState>, IStateGAgent<TState>
    where TState : StateBase, new()
    where TEvent : GEventBase
{
    protected readonly ILogger Logger;

    private readonly List<EventWrapperBaseAsyncObserver> _observers = [];

    private IEventDispatcher? EventDispatcher { get; set; }

    protected GAgentBase(ILogger logger)
    {
        Logger = logger;
        EventDispatcher = ServiceProvider.GetService<IEventDispatcher>();
    }

    public async Task ActivateAsync()
    {
    }

    public async Task RegisterAsync(IGAgent gAgent)
    {
        var guid = gAgent.GetPrimaryKey();
        if (gAgent.GetGrainId() == this.GetGrainId())
        {
            Logger.LogError($"Cannot register GAgent with same GrainId.");
            return;
        }

        await AddChildAsync(gAgent.GetGrainId());
        await gAgent.SubscribeToAsync(this);
        await OnRegisterAgentAsync(guid);
    }

    public Task SubscribeToAsync(IGAgent gAgent)
    {
        return SetParentAsync(gAgent.GetGrainId());
    }

    public async Task UnregisterAsync(IGAgent gAgent)
    {
        await RemoveChildAsync(gAgent.GetGrainId());
        await OnUnregisterAgentAsync(gAgent.GetPrimaryKey());
    }

    public async Task<List<Type>?> GetAllSubscribedEventsAsync(bool includeBaseHandlers = false)
    {
        var eventHandlerMethods = GetEventHandlerMethods();
        eventHandlerMethods = eventHandlerMethods.Where(m =>
            m.Name != nameof(ForwardEventAsync) && m.Name != AevatarGAgentConstants.InitializeDefaultMethodName);
        var handlingTypes = eventHandlerMethods
            .Select(m => m.GetParameters().First().ParameterType);
        if (!includeBaseHandlers)
        {
            handlingTypes = handlingTypes.Where(t => t != typeof(RequestAllSubscriptionsEvent));
        }

        return handlingTypes.ToList();
    }

    public async Task<List<GrainId>> GetChildrenAsync()
    {
        return State.Children;
    }

    public async Task<GrainId> GetParentAsync()
    {
        return State.Parent;
    }

    public virtual Task<Type?> GetInitializeDtoTypeAsync()
    {
        return Task.FromResult(State.InitializeDtoType);
    }

    [EventHandler]
    public async Task<SubscribedEventListEvent> HandleRequestAllSubscriptionsEventAsync(
        RequestAllSubscriptionsEvent request)
    {
        return await GetGroupSubscribedEventListEvent();
    }

    private async Task<SubscribedEventListEvent> GetGroupSubscribedEventListEvent()
    {
        var gAgentList = State.Children.Select(grainId => GrainFactory.GetGrain<IGAgent>(grainId)).ToList();

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
        Logger.LogInformation(
            $"{this.GetGrainId().ToString()} is forwarding event downwards: {JsonConvert.SerializeObject((EventWrapper<EventBase>)eventWrapper)}");
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
        await UpdateInitializeDtoType();
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
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
        if (EventDispatcher != null)
        {
            await EventDispatcher.PublishAsync(State, this.GetGrainId().ToString());
        }
    }

    protected sealed override async void RaiseEvent<T>(T @event)
    {
        Logger.LogInformation("base raiseEvent info:{info}", JsonConvert.SerializeObject(@event));
        base.RaiseEvent(@event);
        InternalRaiseEventAsync(@event).ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                Logger.LogError(task.Exception, "InternalRaiseEventAsync operation failed");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    private async Task InternalRaiseEventAsync<T>(T @event)
    {
        await HandleRaiseEventAsync();
        //TODO:  need optimize use kafka,ensure Es written successfully
        var gEvent = @event as GEventBase;
        if (EventDispatcher != null)
        {
            await EventDispatcher.PublishAsync(gEvent!, gEvent!.Id.ToString());
        }
    }

    protected virtual async Task HandleRaiseEventAsync()
    {

    }

    public async Task OnNextAsync(EventWrapperBase item, StreamSequenceToken? token = null)
    {
        var eventType = (EventBase)item.GetType().GetProperty(nameof(EventWrapper<EventBase>.Event))?.GetValue(item)!;
        Logger.LogInformation($"{this.GetGrainId().ToString()} is handling event {eventType}");
        var matchedObservers = _observers.Where(observer =>
            observer.ParameterTypeName == eventType.GetType().Name ||
            observer.MethodName == nameof(ForwardEventAsync) ||
            observer.MethodName == nameof(HandleRequestAllSubscriptionsEventAsync)).ToList();
        var parameterTypeNames = string.Join(", ", matchedObservers.Select(observer => observer.ParameterTypeName));
        Logger.LogInformation($"{this.GetGrainId().ToString()} has {matchedObservers.Count} valid observers with parameter types: {parameterTypeNames}");
        await Task.WhenAll(matchedObservers.Select(observer => observer.OnNextAsync(item)));
    }

    public async Task OnCompletedAsync()
    {
        // foreach (var observer in _observers)
        // {
        //     await observer.OnCompletedAsync();
        // }

        await Task.WhenAll(_observers.Select(observer => observer.OnCompletedAsync()));
    }

    public async Task OnErrorAsync(Exception ex)
    {
        // foreach (var observer in _observers)
        // {
        //     await observer.OnErrorAsync(ex);
        // }

        await Task.WhenAll(_observers.Select(observer => observer.OnErrorAsync(ex)));
    }
}