using AISmart.Agents;
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

        var viewStateCollection =
            TestLogViewAdaptor<LogViewAdaptorTestGState, LogViewAdaptorTestGEvent>.SnapshotCollection;
        var eventLogCollection =
            TestLogViewAdaptor<LogViewAdaptorTestGState, LogViewAdaptorTestGEvent>.EventLogCollection;

        await TestHelper.WaitUntilAsync(_ => CheckCount(eventLogCollection, 1));
        await TestHelper.WaitUntilAsync(_ => CheckCount(viewStateCollection, 1));
        eventLogCollection.Count.ShouldBe(1);
        viewStateCollection.Count.ShouldBe(1);

        // Check view & event.
        viewStateCollection.Last().Version.ShouldBe(1);
        viewStateCollection.Last().State.Content.First().Value.Greeting.ShouldBe("First event");
        eventLogCollection.Last().Version.ShouldBe(1);
        eventLogCollection.Last().Event.Greeting.ShouldBe("First event");

        await Silo.DeactivateAsync(logViewGAgent);
        logViewGAgent = await Silo.CreateGrainAsync<LogViewAdaptorTestGAgent>(Guid.NewGuid());

        await publishingGAgent.PublishEventAsync(new NaiveTestEvent
        {
            Greeting = "Second event"
        });

        await TestHelper.WaitUntilAsync(_ => CheckCount(eventLogCollection, 2));

        var viewAdaptorGAgentState = await logViewGAgent.GetStateAsync();
        viewAdaptorGAgentState.Content.Count.ShouldBe(2);

        viewStateCollection.Count.ShouldBe(1);
        // Check views and events.
        viewStateCollection.Last().Version.ShouldBe(2);
        viewStateCollection.Last().State.Content.Last().Value.Greeting.ShouldBe("Second event");

        eventLogCollection.Count.ShouldBe(2);
        eventLogCollection.Last().Version.ShouldBe(2);
        eventLogCollection.Last().Event.Greeting.ShouldBe("Second event");
    }

    private async Task<bool> CheckCount<T>(ICollection<T> collection, int expetedCount)
    {
        return collection.Count == expetedCount;
    }
}