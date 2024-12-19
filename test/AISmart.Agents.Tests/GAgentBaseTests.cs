using AISmart.Agents;
using AISmart.EventSourcing.Core;
using AISmart.Grains.Tests.TestEvents;
using AISmart.Grains.Tests.TestGAgents;
using Shouldly;

namespace AISmart.Grains.Tests;

[Trait("Category", "BVT")]
public class GAgentBaseTests : GAgentTestKitBase
{
    [Fact]
    public async Task EventHandlerTest()
    {
        var eventHandlerTestGAgent = await Silo.CreateGrainAsync<EventHandlerTestGAgent>(Guid.NewGuid());
        var subscribedEventList = await eventHandlerTestGAgent.GetAllSubscribedEventsAsync();
        subscribedEventList.ShouldNotBeNull();
        subscribedEventList.Count.ShouldBe(3);
        subscribedEventList.ShouldContain(typeof(NaiveTestEvent));
        subscribedEventList.ShouldContain(typeof(EventWrapperBase));
    }

    [Fact]
    public async Task PublishToEventHandlerTest()
    {
        var eventHandlerTestGAgent = await Silo.CreateGrainAsync<EventHandlerTestGAgent>(Guid.NewGuid());
        var groupGAgent = await CreateGroupGAgentAsync(eventHandlerTestGAgent);
        var publishingGAgent = await CreatePublishingGAgentAsync(groupGAgent);
        await publishingGAgent.PublishEventAsync(new NaiveTestEvent
        {
            Greeting = "Hello world"
        });
        var state = await eventHandlerTestGAgent.GetStateAsync();
        state.Content.Count.ShouldBe(3);
        state.Content.ShouldContain("Hello world");
    }

    [Fact]
    public async Task EventWithResponseTest()
    {
        var eventHandlerTestGAgent = await Silo.CreateGrainAsync<EventHandlerTestGAgent>(Guid.NewGuid());
        var eventHandlerWithResponseTestGAgent =
            await Silo.CreateGrainAsync<EventHandlerWithResponseTestGAgent>(Guid.NewGuid());
        var groupGAgent = await CreateGroupGAgentAsync(eventHandlerTestGAgent, eventHandlerWithResponseTestGAgent);
        var publishingGAgent = await CreatePublishingGAgentAsync(groupGAgent);
        await publishingGAgent.PublishEventAsync(new ResponseTestEvent
        {
            Greeting = "Hello, this is AISmart."
        });
        // Events will forward to eventHandlerTestGAgent
        var state = await eventHandlerTestGAgent.GetStateAsync();
        state.Content.Count.ShouldBe(4);
        state.Content.ShouldContain("Hello, this is AISmart.");
        state.Content.ShouldContain(content => content.Contains(nameof(ResponseTestEvent)));
        state.Content.ShouldContain(content => content.Contains(nameof(NaiveTestEvent)));
    }

    [Fact]
    public async Task BadEventHandlerTest()
    {
        var badEventHandlerTestGAgent = await Silo.CreateGrainAsync<BadEventHandlerTestGAgent>(Guid.NewGuid());
        var subscribedEventList = await badEventHandlerTestGAgent.GetAllSubscribedEventsAsync();
        subscribedEventList.ShouldNotBeNull();
        subscribedEventList.Count.ShouldBe(0);
    }

    [Fact]
    public async Task RequestSubscribedEventListTest()
    {
        var eventHandlerTestGAgent = await Silo.CreateGrainAsync<EventHandlerTestGAgent>(Guid.NewGuid());
        var eventHandlerWithResponseTestGAgent =
            await Silo.CreateGrainAsync<EventHandlerWithResponseTestGAgent>(Guid.NewGuid());
        var subscribeTestGAgent = await Silo.CreateGrainAsync<SubscribeTestGAgent>(Guid.NewGuid());
        var groupGAgent = await CreateGroupGAgentAsync(eventHandlerTestGAgent, eventHandlerWithResponseTestGAgent,
            subscribeTestGAgent);
        var publishingGAgent = await CreatePublishingGAgentAsync(groupGAgent);

        AddProbesByGrainId(eventHandlerTestGAgent, eventHandlerWithResponseTestGAgent, subscribeTestGAgent, groupGAgent,
            publishingGAgent);

        await publishingGAgent.PublishEventAsync(new RequestAllSubscriptionsEvent());
        var state = await subscribeTestGAgent.GetStateAsync();
        state.SubscriptionInfo.Count.ShouldBe(3);
        state.SubscriptionInfo[typeof(EventHandlerTestGAgent)].Count.ShouldBe(3);
        state.SubscriptionInfo[typeof(EventHandlerWithResponseTestGAgent)].Count.ShouldBe(1);
        state.SubscriptionInfo[typeof(SubscribeTestGAgent)].Count.ShouldBe(1);
    }

    [Fact]
    public async Task LogViewAdaptorTest()
    {
        var logViewGAgent = await Silo.CreateGrainAsync<LogViewAdaptorTestGAgent>(Guid.NewGuid());
        var groupGAgent = await CreateGroupGAgentAsync(logViewGAgent);
        var publishingGAgent = await CreatePublishingGAgentAsync(groupGAgent);

        await publishingGAgent.PublishEventAsync(new NaiveTestEvent
        {
            Greeting = "First event"
        });

        await TestHelper.WaitUntilAsync(_ => CheckCount(1));
        InMemoryLogConsistentStorage.Storage.Count.ShouldBe(1);
        InMemoryLogConsistentStorage.Storage.First().Value.Count.ShouldBe(1);

        await Silo.DeactivateAsync(logViewGAgent);
        //logViewGAgent = await Silo.CreateGrainAsync<LogViewAdaptorTestGAgent>(Guid.NewGuid());

        await publishingGAgent.PublishEventAsync(new NaiveTestEvent
        {
            Greeting = "Second event"
        });

        await TestHelper.WaitUntilAsync(_ => CheckCount(2));

        InMemoryLogConsistentStorage.Storage.Count.ShouldBe(1);
        InMemoryLogConsistentStorage.Storage.First().Value.Count.ShouldBe(2);

        var logViewGAgentState = await logViewGAgent.GetStateAsync();
        //logViewGAgentState.Content.Count.ShouldBe(2);
    }

    private async Task<bool> CheckCount(int expectedCount)
    {
        return InMemoryLogConsistentStorage.Storage.Count == expectedCount;
    }
}