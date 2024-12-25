using System;
using System.Linq;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.Agents.AutoGen;
using AISmart.Agents.Developer;
using AISmart.Agents.Group;
using AISmart.Agents.Investment;
using AISmart.Agents.MarketLeader;
using AISmart.Agents.MockA.Events;
using AISmart.Agents.X;
using AISmart.Agents.X.Events;
using AISmart.Application.Grains.Agents.MockA;
using AISmart.Application.Grains.Agents.MockB;
using AISmart.Application.Grains.Agents.MockC;
using AISmart.Application.Grains.Agents.Draw;
using AISmart.Application.Grains.Agents.Math;
using AISmart.GAgent.Autogen;
using AISmart.Sender;
using Orleans;
using Orleans.Runtime;
using Volo.Abp.Application.Services;

namespace AISmart.Application;

public interface IDemoAppService
{
    Task<string> PipelineDemoAsync(string content);
    Task AgentLoadTest(int aGAgentCount, int bGAgentCount, int cGAgentCount);
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
        var xAgent = _clusterClient.GetGrain<IStateGAgent<XAgentState>>(Guid.NewGuid());
        var marketLeaderAgent =
            _clusterClient.GetGrain<IStateGAgent<MarketLeaderAgentState>>(Guid.NewGuid());
        var developerAgent =
            _clusterClient.GetGrain<IStateGAgent<DeveloperAgentState>>(Guid.NewGuid());
        var investmentAgent =
            _clusterClient.GetGrain<IStateGAgent<InvestmentAgentState>>(Guid.NewGuid());
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
        var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());

        await groupAgent.RegisterAsync(xAgent);
        await groupAgent.RegisterAsync(marketLeaderAgent);
        await groupAgent.RegisterAsync(developerAgent);
        await groupAgent.RegisterAsync(investmentAgent);

        await publishingAgent.PublishToAsync(groupAgent);

        await publishingAgent.PublishEventAsync(new XThreadCreatedEvent
        {
            Id = "mock_x_thread_id",
            Content = content
        });

        var investmentAgentState = await investmentAgent.GetStateAsync();
        return investmentAgentState.Content.First();
    }

    public async Task<string> AutogenGAgentTest()
    {
        var groupGAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
        var autogenGAgent = _clusterClient.GetGrain<IAutogenGAgent>(Guid.NewGuid());
        var publishingGAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
        var drawGAgent = _clusterClient.GetGrain<IStateGAgent<DrawOperationState>>(Guid.NewGuid());
        var mathGAgent = _clusterClient.GetGrain<IStateGAgent<MathOperationState>>(Guid.NewGuid());

        autogenGAgent.RegisterAgentEvent(typeof(DrawOperationGAgent), [typeof(DrawTriangleEvent)]);
        autogenGAgent.RegisterAgentEvent(typeof(MathOperationGAgent), [typeof(AddNumberEvent), typeof(SubNumberEvent)]);

        await groupGAgent.RegisterAsync(autogenGAgent);
        await groupGAgent.RegisterAsync(drawGAgent);
        await groupGAgent.RegisterAsync(mathGAgent);
        await groupGAgent.RegisterAsync(publishingGAgent);
        // await groupGAgent.Register(groupGAgent);

        await publishingGAgent.PublishEventAsync(new AutoGenCreatedEvent
        {
            Content = "What is 4+3, and then generate the corresponding polygon?"
        });

        await Task.Delay(10000);

        return "aa";
    }

    public async Task AgentLoadTest(int mockAGAgentCount, int mockBGAgentCount, int mockCGAgentCount)
    {
        var groupGAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());

        for (int i = 0; i < mockAGAgentCount; i++)
        {
            var aGAgent = _clusterClient.GetGrain<IStateGAgent<MockAGAgent>>(Guid.NewGuid());
            await groupGAgent.RegisterAsync(aGAgent);
        }

        for (int i = 0; i < mockBGAgentCount; i++)
        {
            var bGAgent = _clusterClient.GetGrain<IStateGAgent<MockBGAgent>>(Guid.NewGuid());
            await groupGAgent.RegisterAsync(bGAgent);
        }

        for (int i = 0; i < mockCGAgentCount; i++)
        {
            var cGAgent = _clusterClient.GetGrain<IStateGAgent<MockCGAgent>>(Guid.NewGuid());
            await groupGAgent.RegisterAsync(cGAgent);
        }

        await publishingAgent.PublishToAsync(groupGAgent);

        await publishingAgent.PublishEventAsync(new MockAThreadCreatedEvent
        {
            Id = $"mock_A_thread_id",
            Content = $"Call mockAGAgent"
        });
    }
}