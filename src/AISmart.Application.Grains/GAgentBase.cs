using System.Collections.Concurrent;
using System.Reflection;
using AISmart.Agents;
using AISmart.Dapr;
using Microsoft.Extensions.Logging;
using Orleans.EventSourcing;
using Orleans.Providers;
using Orleans.Streams;

namespace AISmart.Application.Grains;

[GAgent]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public abstract class GAgentBase<TState, TEvent> : JournaledGrain<TState, TEvent>, IStateGAgent<TState>
    where TState : class, new()
    where TEvent : GEventBase
{
    public IPersistentState<Dictionary<Guid, string>>? Subscribers { get; }
    private IStreamProvider StreamProvider => this.GetStreamProvider(CommonConstants.StreamProvider);

    protected readonly ILogger Logger;

    // need to use persistent storage to store this
    private readonly Dictionary<Guid, IAsyncStream<EventWrapperBase>> _subscriptions = new();
    private readonly Dictionary<Guid, IAsyncStream<EventWrapperBase>> _publishers = new();
    private readonly List<EventWrapperBaseAsyncObserver> _observers = new();

    protected GAgentBase(ILogger logger, [PersistentState("subscribers")] IPersistentState<Dictionary<Guid, string>>? subscribers = null)
    {
        Subscribers = subscribers;
        Logger = logger;
    }

    public Task ActivateAsync()
    {
        //do nothing
        return Task.CompletedTask;
    }

    public async Task<bool> SubscribeTo(IGAgent agent)
    {
        var agentGuid = agent.GetPrimaryKey();
        var streamId = StreamId.Create(CommonConstants.StreamNamespace, agentGuid);
        var stream = StreamProvider.GetStream<EventWrapperBase>(streamId);
        if (!_subscriptions.TryAdd(agentGuid, stream))
        {
            return false;
        }

        await SubscribeAsync(stream);
        return true;
    }

    public Task<bool> UnsubscribeFrom(IGAgent agent)
    {
        var agentGuid = agent.GetPrimaryKey();
        if (!_subscriptions.ContainsKey(agentGuid))
        {
            return Task.FromResult(false);
        }

        _subscriptions.Remove(agentGuid);
        //TODO: Unsubscribe from stream
        return Task.FromResult(true);
    }

    public Task<bool> PublishTo(IGAgent agent)
    {
        var agentGuid = agent.GetPrimaryKey();
        var streamId = StreamId.Create(CommonConstants.StreamNamespace, agentGuid);
        var stream = StreamProvider.GetStream<EventWrapperBase>(streamId);
        return Task.FromResult(_publishers.TryAdd(agentGuid, stream));
    }

    public Task<bool> UnpublishFrom(IGAgent agent)
    {
        if (!_publishers.ContainsKey(agent.GetPrimaryKey()))
        {
            return Task.FromResult(false);
        }

        _publishers.Remove(agent.GetPrimaryKey());
        return Task.FromResult(true);
    }

    public async Task Register(IGAgent agent)
    {
        var success = await agent.SubscribeTo(this);
        success = await agent.PublishTo(this) | success;

        if (!success)
        {
            return;
        }

        var guid = agent.GetPrimaryKey();

        if (Subscribers != null)
        {
            Subscribers?.State.Add(guid, agent.GetType().Namespace!);
            await Subscribers?.WriteStateAsync()!;
        }

        await OnRegisterAgentAsync(guid);
    }

    public async Task Unregister(IGAgent agent)
    {
        var success = await agent.UnsubscribeFrom(this);
        success = await agent.UnpublishFrom(this) | success;

        if (!success)
        {
            return;
        }

        await OnUnregisterAgentAsync(agent.GetPrimaryKey());
    }

    public async Task<List<Type>?> GetAllSubscribedEventsAsync()
    {
        var eventHandlerMethods = GetEventHandlerMethods();
        var handlingTypes = eventHandlerMethods
            .Select(m => m.GetParameters().First().ParameterType)
            .Where(t => t != typeof(RequestAllSubscriptionsEvent))
            .ToList();
        return handlingTypes;
    }

    [EventHandler]
    public async Task<SubscribedEventListEvent> HandleRequestAllSubscriptionsEventAsync(RequestAllSubscriptionsEvent request)
    {
        if (Subscribers == null)
        {
            return new SubscribedEventListEvent
            {
                GAgentType = GetType()
            };
        }
        var gAgentList = Subscribers.State.Select(keyPair => GrainFactory.GetGrain<IGAgent>(keyPair.Key, keyPair.Value)).ToList();

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

    // protected Task SubscribeAsync<TEventWithResponse, TResponseEvent>(Func<TEventWithResponse, Task<TResponseEvent>> onEvent) 
    //     where TEventWithResponse : EventWithResponseBase<TResponseEvent>
    //     where TResponseEvent : EventBase
    // {
    //     _observers.Add(OnNextWrapperAsync);
    //     return Task.CompletedTask;
    //
    //     async Task OnNextWrapperAsync(EventWrapperBase @event, StreamSequenceToken token = null)
    //     {
    //         Logger.LogInformation("Received message: {@Message}", @event);
    //         if(@event is EventWrapper<TEventWithResponse> eventWrapper)
    //         {
    //             Logger.LogInformation("Received EventWrapper message: {@Message}", eventWrapper);
    //
    //             var response = await onEvent(eventWrapper.Event);
    //             
    //             var responseWrapper = new EventWrapper<TResponseEvent>(response, eventWrapper.EventId);
    //             
    //             await PublishAsync(responseWrapper);
    //         }
    //     }
    // }

    public abstract Task<string> GetDescriptionAsync();

    public Task<TState> GetStateAsync()
    {
        return Task.FromResult(State);
    }

    protected async Task<Guid> PublishAsync<T>(T @event) where T : EventBase
    {
        var eventId = Guid.NewGuid();
        var eventWrapper = new EventWrapper<T>(@event, eventId, this.GetPrimaryKey());

        await PublishAsync(eventWrapper);

        return eventId;
    }

    private async Task PublishAsync<T>(EventWrapper<T> eventWrapper) where T : EventBase
    {
        if (_publishers.Count == 0)
        {
            return;
        }

        foreach (var publisher in _publishers.Select(kp => kp.Value))
        {
            await publisher.OnNextAsync(eventWrapper);
        }
    }

    private async Task AckAsync(EventWrapper<TEvent> eventWrapper)
    {
        // var pubAgent = _clusterClient.GetGrain<IAgent<TState>>(eventWrapper.GrainId);
        //
        // try
        // {
        //     await ExecuteAsync(eventWrapper.Event);
        // }
        // catch (Exception e)
        // {
        //     Logger.LogError(e, "Error executing event");
        // }
        //
        // await ((pubAgent as GAgent<TState,TEvent>)!).DoAckAsync(eventWrapper);
    }

    private async Task DoAckAsync(EventWrapper<TEvent> eventWrapper)
    {
        // eventWrapper.count ++;
        //
        // RaiseEvent(eventWrapper.Event);
        // await ConfirmEvents();
        //
        // var stream = this.GetStreamProvider(CommonConstants.StreamProvider)
        //     .GetStream<EventWrapperBase>(StreamId);
        // var subscriptionHandles =  stream.GetAllSubscriptionHandles();
        // // The count of current subscriptions (consumers).
        // var subscriberCount = subscriptionHandles.Result.Count;
        //
        // if (eventWrapper.count == subscriberCount)
        // {
        //     await CompleteAsync(eventWrapper.Event);
        // }
    }




    // Agent3 -> Agent2 -> Agent3 dependencies through messages
    // strong typed messages
    // 3 messages sub -> 2 messages pub -> 3 messages pub

    // twitter and telegram as a agent
    // twitter and telegram agent publish message
    // market leader agent subscribe to twitter and telegram agent
    // market leader agent publish message to agentA and agentB
    // agentA (investment) and agentB (developer) subscribe to market leader agent
    // market leader agent waits for agentA and agentB to complete
    // market leader agent execute something
    // market leader agent publish message to twitter and telegram agent

    // variation 2
    // agentB sends to agentC to do transaction on chain

    // how does dependency work between agents? just pub/sub messages enough?

    private async Task SubscribeAsync(IAsyncStream<EventWrapperBase> stream)
    {
        foreach (var observer in _observers)
        {
            await stream.SubscribeAsync(observer);
        }
    }

    private async Task HandleEventAsync(EventWrapperBase item)
    {
        Logger.LogInformation("Received message: {@Message}", item);
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await UpdateObserverList();
    }

    private Task UpdateObserverList()
    {
        var eventHandlerMethods = GetEventHandlerMethods();

        foreach (var eventHandlerMethod in eventHandlerMethods)
        {
            var observer = new EventWrapperBaseAsyncObserver(async item =>
            {
                var grainId = (Guid)item.GetType().GetProperty(nameof(EventWrapper<object>.GrainId))?.GetValue(item)!;
                if (grainId == this.GetPrimaryKey())
                {
                    // Skip the event if it is sent by itself.
                    return;
                }
                
                var eventId = (Guid)item.GetType().GetProperty(nameof(EventWrapper<object>.EventId))?.GetValue(item)!;
                var eventType = item.GetType().GetProperty(nameof(EventWrapper<object>.Event))?.GetValue(item);
                var parameter = eventHandlerMethod.GetParameters()[0];
                if (parameter.ParameterType == eventType!.GetType())
                {
                    await HandleMethodInvocationAsync(eventHandlerMethod, parameter, eventType, eventId);
                }

                if (parameter.ParameterType == typeof(EventWrapperBase))
                {
                    try
                    {
                        var invokeParameter = new EventWrapper<EventBase>((EventBase)eventType, eventId, this.GetPrimaryKey());
                        var result = eventHandlerMethod.Invoke(this, [invokeParameter]);
                        await (Task)result!;
                    }
                    catch (Exception ex)
                    {
                        // TODO: Make this better.
                        Logger.LogError(ex, "Error invoking method {MethodName} with event type {EventType}", eventHandlerMethod.Name, eventType.GetType().Name);
                    }
                }
            });

            _observers.Add(observer);
        }

        return Task.CompletedTask;
    }

    private IEnumerable<MethodInfo> GetEventHandlerMethods()
    {
        return GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(IsEventHandlerMethod);
    }

    private bool IsEventHandlerMethod(MethodInfo methodInfo)
    {
        return methodInfo.GetParameters().Length == 1 && (
            // Either the method has the EventHandlerAttribute
            // Or is named HandleEventAsync
            //     and the parameter is not EventWrapperBase 
            //     and the parameter is inherited from EventBase
            ((methodInfo.GetCustomAttribute<EventHandlerAttribute>() != null ||
              methodInfo.Name == nameof(HandleEventAsync)) &&
             methodInfo.GetParameters()[0].ParameterType != typeof(EventWrapperBase) &&
             typeof(EventBase).IsAssignableFrom(methodInfo.GetParameters()[0].ParameterType))
            // Or the method has the AllEventHandlerAttribute and the parameter is EventWrapperBase
            || (methodInfo.GetCustomAttribute<AllEventHandlerAttribute>() != null &&
                methodInfo.GetParameters()[0].ParameterType == typeof(EventWrapperBase)));
    }

    private async Task HandleMethodInvocationAsync(MethodInfo method, ParameterInfo parameter, object eventType, Guid eventId)
    {
        if (IsEventWithResponse(parameter))
        {
            await HandleEventWithResponseAsync(method, eventType, eventId);
        }
        else if (method.ReturnType == typeof(Task))
        {
            try
            {
                var result = method.Invoke(this, [eventType]);
                await (Task)result!;
            }
            catch (Exception ex)
            {
                // TODO: Make this better.
                Logger.LogError(ex, "Error invoking method {MethodName} with event type {EventType}", method.Name, eventType.GetType().Name);
            }
        }
    }

    private bool IsEventWithResponse(ParameterInfo parameter)
    {
        return parameter.ParameterType.BaseType is { IsGenericType: true } &&
               parameter.ParameterType.BaseType.GetGenericTypeDefinition() == typeof(EventWithResponseBase<>);
    }

    private async Task HandleEventWithResponseAsync(MethodInfo method, object eventType, Guid eventId)
    {
        if (method.ReturnType.IsGenericType &&
            method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var resultType = method.ReturnType.GetGenericArguments()[0];
            if (typeof(EventBase).IsAssignableFrom(resultType))
            {
                try
                {
                    var eventResult = await (dynamic)method.Invoke(this, [eventType])!;
                    var eventWrapper = new EventWrapper<EventBase>(eventResult, eventId, this.GetPrimaryKey());
                    await PublishAsync(eventWrapper);
                }
                catch (Exception ex)
                {
                    // TODO: Make this better.
                    Logger.LogError(ex, "Error invoking method {MethodName} with event type {EventType}", method.Name, eventType.GetType().Name);
                }
            }
            else
            {
                throw new InvalidOperationException(
                    $"The event handler of {eventType.GetType()}'s return type needs to be inherit from EventBase.");
            }
        }
        else
        {
            throw new InvalidOperationException(
                $"The event handler of {eventType.GetType()} needs to have a return value.");
        }
    }
}