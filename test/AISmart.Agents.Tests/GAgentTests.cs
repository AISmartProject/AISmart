using AISmart.Agent;
using AISmart.Agent.Events;
using AISmart.Agent.Grains;
using AISmart.Agents.X.Events;
using AISmart.Application.Grains.Agents.Developer;
using AISmart.Application.Grains.Agents.Group;
using AISmart.Application.Grains.Agents.Investment;
using AISmart.Application.Grains.Agents.MarketLeader;
using AISmart.Application.Grains.Agents.Publisher;
using AISmart.Application.Grains.Agents.X;
using AISmart.Events;
using AISmart.Sender;
using Orleans.TestKit;
using Shouldly;

namespace AISmart.Grains.Tests;

public class GAgentTests : GAgentTestKitBase
{
    [Fact]
    public async Task GroupTest()
    {
        var xGAgent = await Silo.CreateGrainAsync<XGAgent>(Guid.NewGuid());
        var marketLeaderGAgent = await Silo.CreateGrainAsync<MarketLeaderGAgent>(Guid.NewGuid());
        var developerGAgent = await Silo.CreateGrainAsync<DeveloperGAgent>(Guid.NewGuid());
        var investmentGAgent = await Silo.CreateGrainAsync<InvestmentGAgent>(Guid.NewGuid());

        var groupGAgent = await CreateGroupGAgentAsync(xGAgent, marketLeaderGAgent, developerGAgent, investmentGAgent);

        var publishingGAgent = await CreatePublishingGAgentAsync(groupGAgent);

        var xThreadCreatedEvent = new XThreadCreatedEvent
        {
            Id = "mock_x_thread_id",
            Content = "BTC REACHED 100k WOOHOOOO!"
        };

        AddProbes(xGAgent, groupGAgent, marketLeaderGAgent, developerGAgent, investmentGAgent);

        await publishingGAgent.PublishEventAsync(xThreadCreatedEvent);

        var xAgentState = await xGAgent.GetStateAsync();
        xAgentState.ThreadIds.Count.ShouldBe(1);

        var investmentAgentState = await investmentGAgent.GetStateAsync();
        investmentAgentState.Content.Count.ShouldBe(1);

        var developerAgentState = await developerGAgent.GetStateAsync();
        developerAgentState.Content.Count.ShouldBe(1);
    }

    [Fact]
    public async Task SendTransactionTest()
    {
        const string chainId = "AELF";
        const string senderName = "Test";
        var createTransactionEvent = new CreateTransactionEvent
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
        Silo.AddProbe<IPublishingGAgent>(_ => publishingAgent);

        await aelfGAgent.ExecuteTransactionAsync(createTransactionEvent);

        var aelfGAgentState = await aelfGAgent.GetAElfAgentDto();
        aelfGAgentState.PendingTransactions.Count.ShouldBe(1);
    }
    
    [Fact]
    public async Task ReceiveMessageTest()
    {
       
        var guid = Guid.NewGuid();
        var groupAgent = await Silo.CreateGrainAsync<GroupGAgent>(Guid.NewGuid());
        var telegramGAgent = await Silo.CreateGrainAsync<TelegramGAgent>(guid);
        await groupAgent.Register(telegramGAgent);
        var txGrain = await Silo.CreateGrainAsync<TelegramGrain>(guid);
        Silo.AddProbe<ITelegramGrain>(_ => txGrain);
        var publishingAgent = await Silo.CreateGrainAsync<PublishingGAgent>(guid);
        await publishingAgent.PublishTo(groupAgent);
        Silo.AddProbe<IPublishingGAgent>(_ => publishingAgent);
        await publishingAgent.PublishEventAsync(new ReceiveMessageEvent
        {
            MessageId = "11",
            ChatId = "12",
            Message = "Test",
            NeedReplyBotName = "Test"
        });
    }
    
    [Fact]
    public async Task SendMessageTest()
    {
        var guid = Guid.NewGuid();
        var groupAgent = await Silo.CreateGrainAsync<GroupGAgent>(Guid.NewGuid());
        var telegramGAgent = await Silo.CreateGrainAsync<TelegramGAgent>(guid);
        await groupAgent.Register(telegramGAgent);
        var txGrain = await Silo.CreateGrainAsync<TelegramGrain>(guid);
        Silo.AddProbe<ITelegramGrain>(_ => txGrain);
        var publishingAgent = await Silo.CreateGrainAsync<PublishingGAgent>(guid);
        await publishingAgent.PublishTo(groupAgent);
        Silo.AddProbe<IPublishingGAgent>(_ => publishingAgent);
        await publishingAgent.PublishEventAsync(new SendMessageEvent
        {
            ChatId = "12",
            Message = "bot message",
            SenderBotName ="Test",
            ReplyMessageId = "11"
        });
        await Task.Delay(10000);
    }
}