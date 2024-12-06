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

    public async Task<List<CreatedEvent>> CreateAgentTaskAsync( Guid templateId, string param)
    {
        var eventNodeDto = await GrainFactory.GetGrain<IAgent>(templateId).GetEventNodeAsync();
        var createTaskEvent = new CreatedEvent
        {
            TaskId = this.GetPrimaryKey(),
            TemplateId = eventNodeDto.Id,
            Name = eventNodeDto.AgentTopic,
            agentTopic = eventNodeDto.AgentTopic,
            IsCompleted = false,
            Param = param
        };
        base.RaiseEvent(createTaskEvent);
        await ConfirmEvents();
        List<CreatedEvent> taskEvents = new List<CreatedEvent>();
        taskEvents.Add(createTaskEvent);
        return taskEvents;
    }

    public async Task<List<CreatedEvent>> CompletedEventAsync(CreatedEvent createdEvent,
        bool isSuccess, string failReason = null, string result = null)
    {
        List<AgentTaskEvent> events = new List<AgentTaskEvent>();
        var completedTaskEvent = new CompletedEvent
        {
            TemplateId = createdEvent.TemplateId,
            CreatedEventId = createdEvent.Id,
            TaskId = this.GetPrimaryKey(),
            Name = createdEvent.Name,
            IsSuccess = isSuccess,
            agentTopic = createdEvent.agentTopic,
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

    public async Task<AgentTaskDto> GetAgentTaskDetailAsync()
    {
        
        return _objectMapper.Map<AgentTaskState,AgentTaskDto>(State);
    }


    protected override void TransitionState(
        AgentTaskState state, AgentTaskEvent @event)
    {
        switch (@event)
        {
            case CreatedEvent createEvent:
                State.Id = createEvent.TaskId;
                State.ProcessingEvents ??= new List<Guid>();
                State.ProcessingEvents.Add(createEvent.Id);
                break;

            case CompletedEvent completeEvent:
                if (completeEvent.IsSuccess)
                {
                    State.EventResultDictionary ??= new Dictionary<Guid, EventResult>();
                    State.EventResultDictionary.Add(completeEvent.CreatedEventId,
                        new EventResult()
                        {
                           Result = completeEvent.Result,
                           AgentTopic = completeEvent.agentTopic
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

    private async Task<List<CreatedEvent>> GetSubTaskEventsAsync(AgentTaskState state, CompletedEvent completeTaskEvent)
    {
        List<CreatedEvent> taskEvents = new List<CreatedEvent>();
        if (!completeTaskEvent.IsSuccess)
        {
            return taskEvents;
        }
        var eventNodeDto = await GrainFactory.GetGrain<IAgent>(completeTaskEvent.TemplateId).GetEventNodeAsync();
        if (!eventNodeDto.Downstreams.IsNullOrEmpty())
        {
            foreach (var guid in eventNodeDto.Downstreams)
            {
                var childNodeDto = await GrainFactory.GetGrain<IAgent>(guid).GetEventNodeAsync();

               var taskEvent =  new CreatedEvent
                {
                    Id = Guid.NewGuid(),
                    agentTopic = childNodeDto.AgentTopic,
                    TemplateId = guid,
                    TaskId = this.GetPrimaryKey(),
                    Name = completeTaskEvent.Name,
                    IsCompleted = false,
                    Param = completeTaskEvent.Result,
                };
                taskEvents.Add(taskEvent);
            }
        }

        return taskEvents;
    }
}