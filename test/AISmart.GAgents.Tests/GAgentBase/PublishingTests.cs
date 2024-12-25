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
}