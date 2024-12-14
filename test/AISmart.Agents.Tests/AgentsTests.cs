using AISmart.Agent;
using AISmart.Agent.Event;
using AISmart.Agent.GEvents;
using AISmart.Agent.Grains;
using AISmart.Agents;
using AISmart.Agents.AutoGen;
using AISmart.Agents.X.Events;
using AISmart.Application.Grains.Agents.Developer;
using AISmart.Application.Grains.Agents.Group;
using AISmart.Application.Grains.Agents.Investment;
using AISmart.Application.Grains.Agents.MarketLeader;
using AISmart.Application.Grains.Agents.Publisher;
using AISmart.Application.Grains.Agents.X;
using AISmart.Dapr;
using AISmart.GAgent.Autogen;
using AISmart.Sender;
using Orleans.TestKit;
using Shouldly;

namespace AISmart.Grains.Tests;

public class AgentsTests : TestKitBase
{
    [Fact]
    public async Task GroupTest()
    {
        var groupAgent = await Silo.CreateGrainAsync<GroupGAgent>(Guid.NewGuid());
        var publishingAgent = await Silo.CreateGrainAsync<PublishingGAgent>(Guid.NewGuid());
        var xAgent = await Silo.CreateGrainAsync<XGAgent>(Guid.NewGuid());
        var marketLeaderAgent = await Silo.CreateGrainAsync<MarketLeaderGAgent>(Guid.NewGuid());
        var developerAgent = await Silo.CreateGrainAsync<DeveloperGAgent>(Guid.NewGuid());
        var investmentAgent = await Silo.CreateGrainAsync<InvestmentGAgent>(Guid.NewGuid());

        await groupAgent.Register(xAgent);
        await groupAgent.Register(marketLeaderAgent);
        await groupAgent.Register(developerAgent);
        await groupAgent.Register(investmentAgent);

        await publishingAgent.PublishTo(groupAgent);

        var xThreadCreatedEvent = new XThreadCreatedEvent
        {
            Id = "mock_x_thread_id",
            Content = "BTC REACHED 100k WOOHOOOO!"
        };

        await publishingAgent.PublishEventAsync(xThreadCreatedEvent);

        var xAgentState = await xAgent.GetStateAsync();
        xAgentState.ThreadIds.Count.ShouldBe(1);
        
        var investmentAgentState = await investmentAgent.GetStateAsync();
        investmentAgentState.Content.Count.ShouldBe(1);
        
        var developerAgentState = await developerAgent.GetStateAsync();
        developerAgentState.Content.Count.ShouldBe(1);
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

    [Fact]
    public async Task AutogenGAgentTest()
    {
        var guid = Guid.NewGuid();
        var autogenGAgent = await Silo.CreateGrainAsync<AutogenGAgent>(guid);
        
        var publishingAgent = await Silo.CreateGrainAsync<PublishingGAgent>(guid);
        Silo.AddProbe<IPublishingAgent>(_ => publishingAgent);

        await autogenGAgent.SubscribeTo(publishingAgent);
        await publishingAgent.PublishTo(autogenGAgent);

        await publishingAgent.PublishEventAsync(new AutoGenCreatedEvent
        {
            Content = "BTC REACHED 100k WOOHOOOO!"
        });
    }
}