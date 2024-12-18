using AISmart.Agents;
using AISmart.Agents.AutoGen;
using AISmart.GAgent.Autogen.State;
using AutoGen.Core;
using Microsoft.Extensions.Logging;
using Orleans.Providers;
using AISmart.Application.Grains;
using AISmart.Dapr;
using AISmart.GAgent.Autogen.Common;
using AISmart.GAgent.Autogen.Event;
using AISmart.GAgent.Autogen.Events;
using AISmart.GAgent.Autogen.EventSourcingEvent;
using AISmart.GEvents.Autogen;
using AISmart.Provider;
using Orleans.Streams;

namespace AISmart.GAgent.Autogen;

[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class AutogenGAgent : GAgentBase<AutoGenAgentState, AutogenEventBase>, IAutogenGAgent
{
    private IStreamProvider StreamProvider => this.GetStreamProvider(CommonConstants.StreamProvider);

    private readonly IRagProvider _ragProvider;

    private readonly AgentDescriptionManager _agentDescriptionManager;

    public AutogenGAgent(ILogger<AutogenGAgent> logger,
        IRagProvider ragProvider, AgentDescriptionManager agentDescriptionManager) : base(logger)
    {
        _ragProvider = ragProvider;
        _agentDescriptionManager = agentDescriptionManager;
    }

    public void RegisterAgentEvent(Type agent, List<Type> eventTypes)
    {
        _agentDescriptionManager.AddAgentEvents(agent, eventTypes);
    }

    public override Task<string> GetDescriptionAsync()
    {
        throw new NotImplementedException();
    }

    [EventHandler]
    public async Task ExecuteAsync(AutoGenCreatedEvent eventData)
    {
        List<AutogenMessage> history = new List<AutogenMessage>();
        // var ragResponse = await _ragProvider.RetrieveAnswerAsync(eventData.Content);
        var ragResponse = string.Empty;
        if (ragResponse.IsNullOrEmpty() == false)
        {
            history.Add(new AutogenMessage(Role.System.ToString(), ragResponse));
        }

        history.Add(new AutogenMessage(Role.User.ToString(), eventData.Content));

        var grain = GrainFactory.GetGrain<IAutoGenExecutor>(Guid.NewGuid());
        await SubscribeStream(grain);
        _ = grain.ExecuteTaskAsync(new ExecutorTaskInfo() { TaskId = eventData.EventId, History = history });
        
        base.RaiseEvent(new Create()
        {
            Id = eventData.EventId,
            Messages = history,
            CreateTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()
        });

        await PublishAsync(new RequestAllSubscriptionsEvent
        {
            RequestFromGAgentType = GetType()
        });
    }

    [EventHandler]
    public async Task ExecuteAsync(AutoGenExecutorEvent eventData)
    {
        switch (eventData.ExecuteStatus)
        {
            case TaskExecuteStatus.Progressing:
                base.RaiseEvent(new CallerProgressing()
                {
                    Id = eventData.TaskId,
                    CurrentCallInfo = eventData.CurrentCallInfo,
                });
                break;
            case TaskExecuteStatus.Break:
                base.RaiseEvent(new Break()
                {
                    Id = eventData.TaskId,
                    BreakReason = eventData.EndContent
                });
                break;
            case TaskExecuteStatus.Finish:
                base.RaiseEvent(new Complete()
                {
                    Id = eventData.TaskId,
                    Summary = eventData.EndContent
                });
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [AllEventHandler]
    public async Task HandleEventAsync(EventWrapperBase eventWrapperBase)
    {
        if (eventWrapperBase is EventWrapper<EventBase> eventWrapper)
        {
            var eventId = eventWrapper.EventId;
        }
    }

    [EventHandler]
    public async Task SaveSubscribedEventsAsync(SubscribedEventListEvent subscribedEventListEvent)
    {
        var eventList = subscribedEventListEvent.Value;
    }

    private async Task SubscribeStream(IGrainWithGuidKey grain)
    {
        var agentGuid = grain.GetPrimaryKey();
        var streamId = StreamId.Create(CommonConstants.StreamNamespace, agentGuid);
        var stream = StreamProvider.GetStream<AutoGenInternalEventBase>(streamId);
        await stream.SubscribeAsync(async (message, token) =>
        {
            if (message is AutoGenExecutorEvent @event1)
            {
                await ExecuteAsync(@event1);
            }

            if (message is PassThroughExecutorEvent @event2)
            {
                await PublishAsync(@event2.PassThroughData as EventBase);
            }
        });
        
    }
}