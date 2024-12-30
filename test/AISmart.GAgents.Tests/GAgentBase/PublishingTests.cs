using AISmart.GAgents.Tests.TestEvents;
using AISmart.GAgents.Tests.TestGAgents;
using Shouldly;

namespace AISmart.GAgents.Tests.GAgentBase;

[Trait("Category", "BVT")]
public class PublishingTests : GAgentTestKitBase
{
    [Fact(DisplayName = "Event can be published to group members.")]
    public async Task PublishToEventHandlerTest()
    {
        // Arrange.
        var eventHandlerTestGAgent = await Silo.CreateGrainAsync<EventHandlerTestGAgent>(Guid.NewGuid());
        var groupGAgent = await CreateGroupGAgentAsync(eventHandlerTestGAgent);
        var publishingGAgent = await CreatePublishingGAgentAsync(groupGAgent);

        // Act.
        await publishingGAgent.PublishEventAsync(new NaiveTestEvent
        {
            Greeting = "Hello world"
        });

        // Assert.
        var state = await eventHandlerTestGAgent.GetStateAsync();
        state.Content.Count.ShouldBe(3);
        state.Content.ShouldContain("Hello world");
    }

    [Fact(DisplayName = "Event can be published downwards to group members.")]
    public async Task MultiLevelDownwardsTest()
    {
        // Arrange.
        var level31 = await Silo.CreateGrainAsync<EventHandlerTestGAgent>(Guid.NewGuid());
        var level32 = await Silo.CreateGrainAsync<EventHandlerTestGAgent>(Guid.NewGuid());
        var level21 = await Silo.CreateGrainAsync<EventHandlerTestGAgent>(Guid.NewGuid());
        var level22 = await Silo.CreateGrainAsync<EventHandlerTestGAgent>(Guid.NewGuid());
        await level21.RegisterAsync(level31);
        await level21.RegisterAsync(level32);
        var level1 = await CreateGroupGAgentAsync(level21, level22);
        var publishingGAgent = await CreatePublishingGAgentAsync(level1);

        // Act.
        await publishingGAgent.PublishEventAsync(new NaiveTestEvent
        {
            Greeting = "Hello world"
        });

        // Assert.
        var state31 = await level31.GetStateAsync();
        state31.Content.Count.ShouldBe(3);
        var state32 = await level32.GetStateAsync();
        state32.Content.Count.ShouldBe(3);
        var state21 = await level21.GetStateAsync();
        state21.Content.Count.ShouldBe(3);
        var state22 = await level22.GetStateAsync();
        state22.Content.Count.ShouldBe(3);
    }

    [Fact(DisplayName = "Event can be published upwards.")]
    public async Task MultiLevelUpwardsTest()
    {
        // Arrange.
        var level31 = await Silo.CreateGrainAsync<EventHandlerTestGAgent>(Guid.NewGuid());
        var level32 = await Silo.CreateGrainAsync<EventHandlerWithResponseTestGAgent>(Guid.NewGuid());
        var level21 = await Silo.CreateGrainAsync<EventHandlerTestGAgent>(Guid.NewGuid());
        var level22 = await Silo.CreateGrainAsync<EventHandlerTestGAgent>(Guid.NewGuid());
        await level21.RegisterAsync(level31);
        await level21.RegisterAsync(level32);
        var level1 = await CreateGroupGAgentAsync(level21, level22);
        var publishingGAgent = await CreatePublishingGAgentAsync(level1);

        // Act: ResponseTestEvent will cause level32 publish an NaiveTestEvent.
        await publishingGAgent.PublishEventAsync(new ResponseTestEvent
        {
            Greeting = "Hello world"
        });

        // Assert: level31 and level21 should receive the response event, then has 1 + 3 content stored.
        var state31 = await level31.GetStateAsync();
        state31.Content.Count.ShouldBe(4);
        var state21 = await level21.GetStateAsync();
        state21.Content.Count.ShouldBe(4);

        // Assert: level22 should not receive the response event, then has 1 content stored (due to [AllEventHandler]).
        var state22 = await level22.GetStateAsync();
        state22.Content.Count.ShouldBe(1);
    }
}