using AISmart.Agent;
using AISmart.Agent.Grains;
using AISmart.Application.Grains.Agents.Group;
using AISmart.Application.Grains.Agents.Publisher;
using AISmart.Events;
using AISmart.Grains;
using AISmart.Sender;
using Orleans.TestKit;

namespace AISmart.GAgents.Tests.PumpFun;

public class PumpFunGAgentTests : GAgentTestKitBase
{
    [Fact]
    public async Task ReceiveMessageTest()
    {
        var guid = Guid.NewGuid();
        var groupAgent = await Silo.CreateGrainAsync<GroupGAgent>(Guid.NewGuid());
        var pumpFunGAgent = await Silo.CreateGrainAsync<PumpFunGAgent>(guid);
        await groupAgent.RegisterAsync(pumpFunGAgent);
        var txGrain = await Silo.CreateGrainAsync<PumpFunGrain>(guid);
        Silo.AddProbe<IPumpFunGrain>(_ => txGrain);
        var publishingAgent = await Silo.CreateGrainAsync<PublishingGAgent>(guid);
        await publishingAgent.PublishToAsync(groupAgent);
        Silo.AddProbe<IPublishingGAgent>(_ => publishingAgent);
        await publishingAgent.PublishEventAsync(new ReceiveMessageEvent
        {
            MessageId = "11",
            ChatId = "12",
            Message = "Test"
        });
    }

    [Fact]
    public async Task SendMessageTest()
    {
        var guid = Guid.NewGuid();
        var groupAgent = await Silo.CreateGrainAsync<GroupGAgent>(Guid.NewGuid());
        var pumpFunGAgent = await Silo.CreateGrainAsync<PumpFunGAgent>(guid);
        await groupAgent.RegisterAsync(pumpFunGAgent);
        var txGrain = await Silo.CreateGrainAsync<PumpFunGrain>(guid);
        Silo.AddProbe<IPumpFunGrain>(_ => txGrain);
        var publishingAgent = await Silo.CreateGrainAsync<PublishingGAgent>(guid);
        await publishingAgent.PublishToAsync(groupAgent);
        Silo.AddProbe<IPublishingGAgent>(_ => publishingAgent);
        await publishingAgent.PublishEventAsync(new SendMessageEvent
        {
            ChatId = "12",
            Message = "bot message",
            BotName = "Test",
            ReplyMessageId = "11"
        });
    }
}