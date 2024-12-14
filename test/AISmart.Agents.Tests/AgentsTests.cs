using AISmart.Agent;
using AISmart.Agent.Event;
using AISmart.Agent.GEvents;
using AISmart.Agent.Grains;
using AISmart.Agents;
using AISmart.Agents.X.Events;
using AISmart.Application.Grains.Agents.Developer;
using AISmart.Application.Grains.Agents.Group;
using AISmart.Application.Grains.Agents.Investment;
using AISmart.Application.Grains.Agents.MarketLeader;
using AISmart.Application.Grains.Agents.Publisher;
using AISmart.Application.Grains.Agents.X;
using AISmart.Dapr;
using AISmart.Sender;
using Orleans.TestKit;
using Shouldly;

namespace AISmart.Grains.Tests;

public class AgentsTests : TestKitBase, IAsyncLifetime
{
    private GroupGAgent _groupAgent;
    private PublishingGAgent _publishingAgent;
    private XGAgent _xAgent;
    private MarketLeaderGAgent _marketLeaderAgent;
    private DeveloperGAgent _developerAgent;
    private InvestmentGAgent _investmentAgent;

    public async Task InitializeAsync()
    {
        _groupAgent = await Silo.CreateGrainAsync<GroupGAgent>(Guid.NewGuid());
        _publishingAgent = await Silo.CreateGrainAsync<PublishingGAgent>(Guid.NewGuid());
        _xAgent = await Silo.CreateGrainAsync<XGAgent>(Guid.NewGuid());
        _marketLeaderAgent = await Silo.CreateGrainAsync<MarketLeaderGAgent>(Guid.NewGuid());
        _developerAgent = await Silo.CreateGrainAsync<DeveloperGAgent>(Guid.NewGuid());
        _investmentAgent = await Silo.CreateGrainAsync<InvestmentGAgent>(Guid.NewGuid());

        await _groupAgent.Register(_xAgent);
        await _groupAgent.Register(_marketLeaderAgent);
        await _groupAgent.Register(_developerAgent);
        await _groupAgent.Register(_investmentAgent);

        await _publishingAgent.PublishTo(_groupAgent);
    }

    [Fact]
    public async Task GroupPublishTest()
    {
        await _publishingAgent.PublishEventAsync(new XThreadCreatedEvent
        {
            Id = "mock_x_thread_id",
            Content = "BTC REACHED 100k WOOHOOOO!"
        });

        var xAgentState = await _xAgent.GetStateAsync();
        xAgentState.ThreadIds.Count.ShouldBe(1);

        var investmentAgentState = await _investmentAgent.GetStateAsync();
        investmentAgentState.Content.Count.ShouldBe(1);

        var developerAgentState = await _developerAgent.GetStateAsync();
        developerAgentState.Content.Count.ShouldBe(1);
    }
    
    [Fact]
    public async Task GroupBatchPublishTest()
    {
        var events = new List<XThreadCreatedEvent>
        {
            new()
            {
                Id = "mock_x_thread_id_1",
                Content = "BTC REACHED 100k WOOHOOOO!"
            },
            new()
            {
                Id = "mock_x_thread_id_2",
                Content = "ETH REACHED 4k WOOHOOOO!"
            }
        };
        await _publishingAgent.PublishEventAsync(events);

        var xAgentState = await _xAgent.GetStateAsync();
        xAgentState.ThreadIds.Count.ShouldBe(2);

        var investmentAgentState = await _investmentAgent.GetStateAsync();
        investmentAgentState.Content.Count.ShouldBe(2);

        var developerAgentState = await _developerAgent.GetStateAsync();
        developerAgentState.Content.Count.ShouldBe(2);
    }

    [Fact]
    public async Task SendTransactionTest()
    {
        const string chainId = "AELF";
        const string senderName = "Test";
        var createTransactionEvent = new CreateTransactionGEvent
        {
            ChainId = chainId,
            SenderName = senderName,
            ContractAddress = "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE",
            MethodName = "Transfer",
        };
        var guid = Guid.NewGuid();
        var aelfGAgent = await Silo.CreateGrainAsync<AElfGAgent>(guid);
        var txGrain = await Silo.CreateGrainAsync<TransactionGrain>(guid);
        Silo.AddProbe<ITransactionGrain>(_ => txGrain);
        var publishingAgent = await Silo.CreateGrainAsync<PublishingGAgent>(guid);
        Silo.AddProbe<IPublishingAgent>(_ => publishingAgent);

        await aelfGAgent.ExecuteTransactionAsync(createTransactionEvent);

        var aelfGAgentState = await aelfGAgent.GetAElfAgentDto();
        aelfGAgentState.PendingTransactions.Count.ShouldBe(1);
    }

    public async Task DisposeAsync()
    {
    }
}