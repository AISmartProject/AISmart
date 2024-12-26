using AISmart.Agent;
using AISmart.Agent.Grains;
using AISmart.Application.Grains.Agents.Group;
using AISmart.Application.Grains.Agents.Publisher;
using AISmart.Events;
using AISmart.GAgents.Tests;
using AISmart.Grains;
using AISmart.Sender;
using Orleans.TestKit;

namespace AISmart.GAgents.Tests.Telegram;

public class TelegramGAgentTests : GAgentTestKitBase
{
    [Fact]
    public async Task ReceiveMessageTest()
    {
        var guid = Guid.NewGuid();
        var groupAgent = await Silo.CreateGrainAsync<GroupGAgent>(Guid.NewGuid());
        var telegramGAgent = await Silo.CreateGrainAsync<TelegramGAgent>(guid);
        await groupAgent.RegisterAsync(telegramGAgent);
        var txGrain = await Silo.CreateGrainAsync<TelegramGrain>(guid.ToString());
        Silo.AddProbe<ITelegramGrain>(_ => txGrain);
        var publishingAgent = await Silo.CreateGrainAsync<PublishingGAgent>(guid);
        await publishingAgent.RegisterAsync(groupAgent);
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
        var telegramGAgent = await Silo.CreateGrainAsync<TelegramGAgent>(guid);
        await groupAgent.RegisterAsync(telegramGAgent);
        var txGrain = await Silo.CreateGrainAsync<TelegramGrain>(guid.ToString());
        Silo.AddProbe<ITelegramGrain>(_ => txGrain);
        var publishingAgent = await Silo.CreateGrainAsync<PublishingGAgent>(guid);
        await publishingAgent.RegisterAsync(groupAgent);
        Silo.AddProbe<IPublishingGAgent>(_ => publishingAgent);
        await publishingAgent.PublishEventAsync(new SendMessageEvent
        {
            ChatId = "12",
            Message = "bot message",
            ReplyMessageId = "11"
        });
    }
}