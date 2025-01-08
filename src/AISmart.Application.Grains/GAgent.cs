using AISmart.Agents;
using AISmart.Dapr;
using Microsoft.Extensions.Logging;
using Orleans.EventSourcing;
using Orleans.Runtime;
using Orleans.Streams;

namespace AISmart.Application.Grains;

public abstract class GAgent<TState, TEvent> : JournaledGrain<TState, TEvent>, IStateAgent<TState>
    where TState : class, new()
    where TEvent : GEvent
{
    protected IStreamProvider? StreamProvider { get; private set; } = null;
    
    protected readonly ILogger Logger;
    
    private readonly IClusterClient _clusterClient;
    // need to use persistent storage to store this
    private readonly Dictionary<Guid, IAsyncStream<EventWrapperBase>> _subscriptions = new();
    private readonly Dictionary<Guid, IAsyncStream<EventWrapperBase>> _publishers = new();
    private readonly List<Func<EventWrapperBase, StreamSequenceToken, Task>> _subscriptionHandlers = new();
    
    protected GAgent(ILogger logger, IClusterClient clusterClient)
    {
        Logger = logger;
        _clusterClient = clusterClient;
    }
    
    public Task ActivateAsync()
    {
        //do nothing
        return Task.CompletedTask;
    }
    
    public async Task<bool> SubscribeTo(IAgent agent)
    {
        StreamProvider ??= this.GetStreamProvider(CommonConstants.StreamProvider);

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

    public async Task<bool> UnsubscribeFrom(IAgent agent)
    {
        var agentGuid = agent.GetPrimaryKey();
        if (!_subscriptions.ContainsKey(agentGuid))
        {
            return false;
        }

        _subscriptions.Remove(agentGuid);
        //TODO: Unsubscribe from stream
        return true;
    }

    public async Task<bool> PublishTo(IAgent agent)
    {
        StreamProvider ??= this.GetStreamProvider(CommonConstants.StreamProvider);

        var agentGuid = agent.GetPrimaryKey();
        var streamId = StreamId.Create(CommonConstants.StreamNamespace, agentGuid);
        var stream = StreamProvider.GetStream<EventWrapperBase>(streamId);
        return _publishers.TryAdd(agentGuid, stream);
    }

    public async Task<bool> UnpublishFrom(IAgent agent)
    {
        if (!_publishers.ContainsKey(agent.GetPrimaryKey()))
        {
            return false;
        }

        _publishers.Remove(agent.GetPrimaryKey());
        return true;
    }

    public async Task Register(IAgent agent)
    {
        var success = await agent.SubscribeTo(this);
        success = await agent.PublishTo(this) | success;
        
        if(!success)
        {
            return;
        }
        
        await OnRegisterAgentAsync(agent.GetPrimaryKey());
    }

    public async Task Unregister(IAgent agent)
    {
        var success = await agent.UnsubscribeFrom(this);
        success = await agent.UnpublishFrom(this) | success;
        
        if(!success)
        {
            return;
        }

        await OnUnregisterAgentAsync(agent.GetPrimaryKey());
    }
    
    protected virtual async Task OnRegisterAgentAsync(Guid agentGuid)
    {
    }
    
    protected virtual async Task OnUnregisterAgentAsync(Guid agentGuid)
    {
    }

    protected async Task SubscribeAsync<T>(Func<T, Task> onEvent)
    {
        _subscriptionHandlers.Add(OnNextWrapperAsync);
        return;

        Task OnNextWrapperAsync(EventWrapperBase @event, StreamSequenceToken token = null)
        {
            Logger.LogInformation("Received message: {@Message}", @event);
            if(@event is EventWrapper<T> eventWrapper)
            {
                Logger.LogInformation("Received EventWrapper message: {@Message}", eventWrapper);

                onEvent(eventWrapper.Event);
                //await DoAckAsync(eventWrapper);
            }

            return Task.CompletedTask;
        }
    }
    
    public abstract Task<string> GetDescriptionAsync();
    
    public Task<TState> GetStateAsync()
    {
        return Task.FromResult(State);
    }

    protected async Task PublishAsync<T>(T @event) where T : GEvent
    {
        if(_publishers.Count == 0)
        {
            return;
        }
        
        var eventWrapper = new EventWrapper<T>(@event, this.GetGrainId());
        
        foreach (var publisher in _publishers.Select(kp => kp.Value))
        {
            await publisher.OnNextAsync(eventWrapper);
        }
    }

    public async Task AckAsync(EventWrapper<TEvent> eventWrapper)
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

    public async Task DoAckAsync(EventWrapper<TEvent> eventWrapper)
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
    

    protected abstract Task ExecuteAsync(TEvent eventData);
    
    protected abstract Task CompleteAsync(TEvent eventData);

    
    private async Task OnNextAsync(EventWrapperBase @event, StreamSequenceToken token = null)
    {
        Logger.LogInformation("Received message: {@Message}", @event);
        if(@event is EventWrapper<TEvent> eventWrapper)
        {
            Logger.LogInformation("Received EventWrapper message: {@Message}", eventWrapper);

            await ExecuteAsync(eventWrapper.Event);
            await DoAckAsync(eventWrapper);
        }
    }
    
    private async Task SubscribeAsync(IAsyncStream<EventWrapperBase> stream)
    {
        foreach (var handler in _subscriptionHandlers)
        {
            await stream.SubscribeAsync(handler);
        }
    }
}