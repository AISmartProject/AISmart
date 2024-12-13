using AISmart.Domain.Grains.Event;
using Orleans.Providers;
using Volo.Abp.ObjectMapping;

namespace AISmart.Application.Grains.Event;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.EventSourcing;
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class AgentAgentTaskGrain : JournaledGrain<AgentTaskState, AgentTaskEvent>, IAgentTaskGrain
{
    private readonly IObjectMapper _objectMapper;

    public AgentAgentTaskGrain(IObjectMapper objectMapper)
    {
        _objectMapper = objectMapper;
    }

    public async Task<List<CreatedAgentEvent>> CreateTask( Guid templateId, string param)
    {
        var eventNodeDto = await GrainFactory.GetGrain<IEventFlowTemplateGrain>(templateId).GetEventNode();
        var createTaskEvent = new CreatedAgentEvent
        {
            TaskId = this.GetPrimaryKey(),
            TemplateId = eventNodeDto.Id,
            Name = eventNodeDto.AgentTopic,
            IsCompleted = false,
            Param = param
        };
        base.RaiseEvent(createTaskEvent);
        await ConfirmEvents();
        List<CreatedAgentEvent> taskEvents = new List<CreatedAgentEvent>();
        taskEvents.Add(createTaskEvent);
        return taskEvents;
    }

    public async Task<List<CreatedAgentEvent>> CompleteEvent(Guid eventId,bool isSuccess, string failReason = null, string result = null)
    {
        List<AgentTaskEvent> events = new List<AgentTaskEvent>();
        var completedTaskEvent = new CompletedAgentEvent
        {
            CreatedEventId = eventId,
            TaskId = this.GetPrimaryKey(),
            IsSuccess = isSuccess,
            FailReason = failReason,
            Result = result
        };
        events.Add(completedTaskEvent);
        var taskEvents =  await GetSubTaskEventsAsync(State, completedTaskEvent);
        if (!taskEvents.IsNullOrEmpty())
        {
            events.AddRange(taskEvents);
        }

        base.RaiseEvents(events);
        await ConfirmEvents();
       
        return  taskEvents;
    }

    public async Task<AgentTaskDto> GetTask()
    {
        return _objectMapper.Map<AgentTaskState,AgentTaskDto>(State);
    }


    protected override void TransitionState(
        AgentTaskState state, AgentTaskEvent @event)
    {
        switch (@event)
        {
            case CreatedAgentEvent createEvent:
                State.Id = createEvent.TaskId;
                State.ProcessingEvents ??= new List<Guid>();
                State.ProcessingEvents.Add(createEvent.Id);
                break;

            case CompletedAgentEvent completeEvent:
                if (completeEvent.IsSuccess)
                {
                    State.EventResultDictionary ??= new Dictionary<Guid, EventResult>();
                    State.EventResultDictionary.Add(completeEvent.CreatedEventId,
                        new EventResult()
                        {
                           Result = completeEvent.Result,
                          // AgentTopic = completeEvent.a
                        });
                }
                else
                {
                    State.FailedEvents ??= new List<Guid>();
                    State.FailedEvents.Add(completeEvent.CreatedEventId);
                }
                State.ProcessingEvents.Remove(completeEvent.CreatedEventId);
                break;
        }
    }

    private async Task<List<CreatedAgentEvent>> GetSubTaskEventsAsync(AgentTaskState state, CompletedAgentEvent completeEvent)
    {
        List<CreatedAgentEvent> taskEvents = new List<CreatedAgentEvent>();
        var eventNodeDto = await GrainFactory.GetGrain<IEventFlowTemplateGrain>(completeEvent.TemplateId).GetEventNode();
        if (!eventNodeDto.Downstreams.IsNullOrEmpty())
        {
            foreach (var guid in eventNodeDto.Downstreams)
            {
               var taskEvent =  new CreatedAgentEvent
                {
                    Id = Guid.NewGuid(),
                    TemplateId = guid,
                    TaskId = this.GetPrimaryKey(),
                    Name = completeEvent.Name,
                    IsCompleted = false,
                    Param = completeEvent.Result
                };
                taskEvents.Add(taskEvent);
            }
        }

        return taskEvents;
    }
}