using System;
using System.Collections.Generic;
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
using AISmart.Application.Grains.Agents.Draw;
using AISmart.Application.Grains.Agents.Math;
using AISmart.Application.Grains.Agents.MockA;
using AISmart.Application.Grains.Agents.MockB;
using AISmart.Application.Grains.Agents.MockC;
using AISmart.GAgent.Autogen;
using AISmart.Sender;
using Orleans;
using Volo.Abp.Application.Services;
using AddNumberEvent = AISmart.Application.Grains.Agents.Math.AddNumberEvent;

namespace AISmart.Application;

public interface IDemoAppService
{
    Task<string> PipelineDemoAsync(string content);
}

public class DemoAppService : ApplicationService, IDemoAppService
{
    private readonly IClusterClient _clusterClient;
    private static List<IMockAGAgentCount> MockAGAgentcount = new List<IMockAGAgentCount>();
    private static List<IMockBGAgentCount> MockBGAgentcount = new List<IMockBGAgentCount>();
    private static List<IMockCGAgentCount> MockCGAgentcount = new List<IMockCGAgentCount>();

    private static DateTime? startTime = null;
    private static DateTime? endTime = null;
    private static TimeSpan? duration = null;

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

    public async Task AgentLoadAsyncTest(int groupGAgentCount, int mockAGAgentCount, int mockBGAgentCount,
        int mockCGAgentCount)
    {
        MockAGAgentcount.Clear();
        MockBGAgentcount.Clear();
        MockCGAgentcount.Clear();

        startTime = DateTime.UtcNow;
        endTime = null;
        duration = null;

        for (int j = 0; j < groupGAgentCount; j++)
        {
            var groupGAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
            var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());

            var tasks = new List<Task>();

            for (int i = 0; i < mockAGAgentCount; i++)
            {
                var aGAgent = _clusterClient.GetGrain<IMockAGAgentCount>(Guid.NewGuid());
                tasks.Add(RegisterAgentAsync(groupGAgent, aGAgent));
                await Task.Delay(2000);
                MockAGAgentcount.Add(aGAgent);
            }

            for (int i = 0; i < mockBGAgentCount; i++)
            {
                var bGAgent = _clusterClient.GetGrain<IMockBGAgentCount>(Guid.NewGuid());
                tasks.Add(RegisterAgentAsync(groupGAgent, bGAgent));
                await Task.Delay(2000);
                MockBGAgentcount.Add(bGAgent);
            }

            for (int i = 0; i < mockCGAgentCount; i++)
            {
                var cGAgent = _clusterClient.GetGrain<IMockCGAgentCount>(Guid.NewGuid());
                tasks.Add(RegisterAgentAsync(groupGAgent, cGAgent));
                await Task.Delay(2000);
                MockCGAgentcount.Add(cGAgent);
            }

            await Task.WhenAll(tasks);

            await publishingAgent.PublishToAsync(groupGAgent);

            await publishingAgent.PublishEventAsync(new MockAThreadCreatedEvent
            {
                Id = $"mock_A_thread_id_{j}",
                Content = $"Call mockAGAgent for group {j + 1}"
            });
        }
    }

    private async Task RegisterAgentAsync(IStateGAgent<GroupAgentState> groupGAgent, IGAgent agent)
    {
        await groupGAgent.RegisterAsync(agent);
    }

    public async Task<string> AgentLoadTestCount(int groupGAgentCount, int mockAGAgentCount, int mockBGAgentCount,
        int mockCGAgentCount)
    {
        int totalMockAGCount = 0;
        int totalMockBGCount = 0;
        int totalMockCGCount = 0;

        foreach (var agent in MockAGAgentcount)
        {
            totalMockAGCount += await agent.GetMockAGAgentCount();
        }

        foreach (var agent in MockBGAgentcount)
        {
            totalMockBGCount += await agent.GetMockBGAgentCount();
        }

        foreach (var agent in MockCGAgentcount)
        {
            totalMockCGCount += await agent.GetMockCGAgentCount();
        }

        if (totalMockCGCount == groupGAgentCount * mockAGAgentCount * mockBGAgentCount * mockCGAgentCount &&
            endTime == null)
        {
            endTime = DateTime.UtcNow;
            duration = endTime.Value - startTime;
        }

        var result = new
        {
            MockAGAgentCount = totalMockAGCount,
            MockBGAgentCount = totalMockBGCount,
            MockCGAgentCount = totalMockCGCount,
            StartTime = startTime,
            EndTime = endTime,
            DurationMs = duration?.TotalMilliseconds ?? 0
        };

        return await Task.FromResult(System.Text.Json.JsonSerializer.Serialize(new
        {
            code = "20000",
            data = result
        }));
    }
}