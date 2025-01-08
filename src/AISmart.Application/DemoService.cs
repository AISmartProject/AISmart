using System;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.Agents.ImplementationAgent.Events;
using AISmart.Agents.MarketLeader.Events;
using AISmart.Agents.X.Events;
using AISmart.Application.Grains.Agents.Developer;
using AISmart.Application.Grains.Agents.MarketLeader;
using AISmart.Application.Grains.Agents.X;
using AISmart.Sender;
using Orleans;
using Volo.Abp.Application.Services;

namespace AISmart.Application;

public interface IDemoAppService
{
    Task<string> PipelineDemoAsync(string content);
}

public class DemoAppService : ApplicationService, IDemoAppService
{
    private readonly IClusterClient _clusterClient;

    public DemoAppService(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    public async Task<string> PipelineDemoAsync(string content)
    {
        var xAgent = _clusterClient.GetGrain<IStateAgent<XAgentState>>(Guid.NewGuid(), typeof(XAgent).Namespace);
        await xAgent.ActivateAsync();

        var marketLeaderAgent = _clusterClient.GetGrain<IStateAgent<MarketLeaderAgentState>>(Guid.NewGuid(), typeof(MarketLeaderAgent).Namespace);
        await marketLeaderAgent.ActivateAsync();

        var developerAgent = _clusterClient.GetGrain<IStateAgent<DeveloperAgentState>>(Guid.NewGuid(), typeof(DeveloperAgent).Namespace);
        await developerAgent.ActivateAsync();

        var publishingAgent = _clusterClient.GetGrain<IPublishingAgent>(Guid.NewGuid());
        await publishingAgent.PublishEventAsync(new XThreadCreatedEvent
        {
            Id = "mock_x_thread_id",
            Content = content
        });

        return content;
    }
}