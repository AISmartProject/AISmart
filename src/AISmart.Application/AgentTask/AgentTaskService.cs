using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Application.Grains.Event;
using AISmart.Dapr;
using AISmart.Domain.Grains.Event;
using Orleans;
using Volo.Abp.Application.Services;

namespace AISmart.AgentTask;

public class AgentTaskService : ApplicationService, IAgentTaskService
{
    private readonly IDaprProvider _daprProvider;
    private readonly IClusterClient _clusterClient;

    public AgentTaskService(IDaprProvider daprProvider, IClusterClient clusterClient)
    {
        _daprProvider = daprProvider;
        _clusterClient = clusterClient;
    }

    public async Task<Guid> CreateAgentTaskAsync(Guid TaskTemplateId, string param)
    {
        var taskId = Guid.NewGuid();
        var eventList = await _clusterClient.GetGrain<IAgentTaskGrain>(taskId)
            .CreateAgentTaskAsync(TaskTemplateId, param);
        if (!eventList.IsNullOrEmpty())
        {
            foreach (var taskEvent in eventList)
            {
                await _daprProvider.PublishEventAsync(CommonConstants.PubSubName, taskEvent.agentTopic, taskEvent);
            }
        }

        return taskId;
    }

    public async Task<Guid> CompletedEventAsync(CreatedEvent createdEvent, bool isSuccess,
        string failReason, string result)
    {
        var eventList = await _clusterClient.GetGrain<IAgentTaskGrain>(createdEvent.TaskId)
            .CompletedEventAsync(createdEvent, isSuccess, failReason, result);
        if (!eventList.IsNullOrEmpty())
        {
            foreach (var taskEvent in eventList)
            {
                await _daprProvider.PublishEventAsync(CommonConstants.PubSubName, taskEvent.agentTopic, taskEvent);
            }
        }

        return createdEvent.TaskId;
    }

    public Task<AgentTaskDto> GetAgentTaskDetailAsync(Guid taskId)
    {
        return _clusterClient.GetGrain<IAgentTaskGrain>(taskId).GetAgentTaskDetailAsync();
    }
}