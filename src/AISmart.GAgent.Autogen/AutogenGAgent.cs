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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Orleans.Runtime;
using Orleans.Streams;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
        Logger.LogInformation(
            $"[AutogenGAgent] received AutoGenCreatedEvent message,taskId is{eventData.EventId}, {JsonSerializer.Serialize(eventData)}");

        List<AutogenMessage> history = new List<AutogenMessage>();
        // var ragResponse = await _ragProvider.RetrieveAnswerAsync(eventData.Content);
        // if (ragResponse.IsNullOrEmpty() == false)
        // {
        //     history.Add(new AutogenMessage(Role.System.ToString(), ragResponse));
        // }

        history.Add(new AutogenMessage(Role.User.ToString(), eventData.Content));

        await PublishEventToExecutor(eventData.EventId, history);

        base.RaiseEvent(new Create()
        {
            TaskId = eventData.EventId,
            Messages = history,
            CreateTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()
        });

        await base.ConfirmEvents();
    }

    [AllEventHandler]
    public async Task HandleEventAsync(EventWrapperBase eventWrapperBase)
    {
        if (eventWrapperBase is EventWrapper<EventBase> eventWrapper)
        {
            var eventId = eventWrapper.EventId;
            if (State.CheckEventIdExist(eventId) == false)
            {
                return;
            }

            Logger.LogInformation(
                $"[AutogenGAgent] receive reply, eventId:{eventId}, receive message is{eventWrapper.Event}");

            var eventInfo = State.GetEventInfoByEventId(eventId);
            if (eventInfo == null)
            {
                Logger.LogWarning(
                    $"[AutogenGAgent] receive reply but not found eventInfo, eventId:{eventId}, receive message is{eventWrapper.Event}");
                return;
            }

            var agentName = eventInfo.AgentName;
            var eventName = eventInfo.EventName;
            var content = JsonConvert.SerializeObject(eventWrapper.Event);
            var reply =
                $"The {eventName} of {agentName} has been processed, the response of {eventName} is: {content}. The input for the next request may depend on the JSON data in the response.";

            var taskInfo = State.GetStateInfoByEventId(eventId);
            if (taskInfo == null)
            {
                Logger.LogWarning(
                    $"[AutogenGAgent] receive reply but not found taskInfo, eventId:{eventId}, taskId{taskInfo.TaskId}, receive message is{eventWrapper.Event}");
                return;
            }

            base.RaiseEvent(new CallAgentReply()
            {
                EventId = eventId,
                Reply = new AutogenMessage(Role.Assistant.ToString(), reply)
            });

            await base.ConfirmEvents();

            if (State.CheckIsRunning(taskInfo.TaskId))
            {
                return;
            }

            await PublishEventToExecutor(taskInfo.TaskId, taskInfo.ChatHistory);
        }
    }

    [EventHandler]
    public async Task HandleEventAsync(SubscribedEventListEvent subscribedEventListEvent)
    {
        if (subscribedEventListEvent == null || subscribedEventListEvent.Value == null)
        {
            return;
        }

        foreach (var (agentType, eventTypeList) in subscribedEventListEvent.Value)
        {
            _agentDescriptionManager.AddAgentEvents(agentType, eventTypeList);
        }
    }

    [EventHandler]
    public async Task ExecuteAsync(AutoGenExecutorEvent eventData)
    {
        switch (eventData.ExecuteStatus)
        {
            case TaskExecuteStatus.Progressing:
                base.RaiseEvent(new CallerProgressing()
                {
                    TaskId = eventData.TaskId,
                    CurrentCallInfo = eventData.CurrentCallInfo,
                });
                break;
            case TaskExecuteStatus.Break:
                Logger.LogInformation(
                    $"[AutogenGAgent] Task Break,TaskId:{eventData.TaskId}, finish content:{eventData.EndContent}");
                base.RaiseEvent(new Break()
                {
                    TaskId = eventData.TaskId,
                    BreakReason = eventData.EndContent
                });
                break;
            case TaskExecuteStatus.Finish:
                Logger.LogInformation(
                    $"[AutogenGAgent] Task Finished,TaskId:{eventData.TaskId}, finish content:{eventData.EndContent}");
                base.RaiseEvent(new Complete()
                {
                    TaskId = eventData.TaskId,
                    Summary = eventData.EndContent
                });
                break;
        }

        await base.ConfirmEvents();
    }

    private async Task PublishEventToExecutor(Guid taskId, List<AutogenMessage> history)
    {
        var grain = GrainFactory.GetGrain<IAutoGenExecutor>(Guid.NewGuid());
        await SubscribeStream(grain);
        _ = grain.ExecuteTaskAsync(new ExecutorTaskInfo() { TaskId = taskId, History = history });
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
                var eventId = await PublishAsync(@event2.PassThroughData as EventBase);

                Logger.LogInformation(
                    $"[AutogenGAgent] Publish Event, EventId{@event2.TaskId.ToString()}, eventId:{eventId.ToString()}, publish content: {JsonSerializer.Serialize(@event2.PassThroughData)}");

                base.RaiseEvent(new PublishEvent()
                {
                    TaskId = @event2.TaskId,
                    EventId = eventId,
                    AgentName = @event2.AgentName,
                    EventName = @event2.EventName
                });

                await base.ConfirmEvents();
            }
        });
    }
}