using AISmart.GAgents.Tests.TestEvents;
using AISmart.GAgents.Tests.TestGAgents;
using Shouldly;

namespace AISmart.GAgents.Tests.GAgentBase;

[Trait("Category", "BVT")]
public class ChattingTests : GAgentTestKitBase
{
    [Fact(Skip = "Implementation reverted.")]
    public async Task ChatIdRelayTest()
    {
        var chatTestGAgent = await Silo.CreateGrainAsync<ChatTestGAgent>(Guid.NewGuid());
        var relayTestGAgent = await Silo.CreateGrainAsync<RelayTestGAgent>(Guid.NewGuid());
        var groupGAgent = await CreateGroupGAgentAsync(chatTestGAgent, relayTestGAgent);
        var publishingGAgent = await CreatePublishingGAgentAsync(groupGAgent);

        await publishingGAgent.PublishEventAsync(new ReceiveMessageTestEvent
        {
            MessageId = "Fake message id",
            ChatId = "origin chat id",
            Message = "Hello, world!",
            BotName = "Fake bot name"
        });

        var chatTestGAgentState = await chatTestGAgent.GetStateAsync();
        chatTestGAgentState.SendMessages.Count.ShouldBe(1);
        chatTestGAgentState.SendMessages["origin chat id"].ShouldBe("I handled a social event: Hello, world!");
    }
}