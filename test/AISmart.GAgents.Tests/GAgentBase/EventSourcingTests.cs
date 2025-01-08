using AISmart.GAgents.Tests.TestEvents;
using AISmart.GAgents.Tests.TestGAgents;
using Shouldly;

namespace AISmart.GAgents.Tests.GAgentBase;

[Trait("Category", "BVT")]
public class EventSourcingTests : GAgentTestKitBase
{
    [Fact(DisplayName = "Implementation of LogViewAdaptor works.")]
    public async Task LogViewAdaptorTest()
    {
        // Arrange.
        var guid = Guid.NewGuid();
        var logViewGAgent = await Silo.CreateGrainAsync<LogViewAdaptorTestGAgent>(guid);
        var groupGAgent = await CreateGroupGAgentAsync(logViewGAgent);
        var publishingGAgent = await CreatePublishingGAgentAsync(groupGAgent);

        AddProbesByGrainId(publishingGAgent, groupGAgent, logViewGAgent);

        // Act: First event.
        await publishingGAgent.PublishEventAsync(new NaiveTestEvent
        {
            Greeting = "First event"
        });

        // Assert.
        await TestHelper.WaitUntilAsync(_ => CheckCount(1));
        Silo.TestLogConsistentStorage.Storage.Count.ShouldBe(1);
        Silo.TestLogConsistentStorage.Storage.First().Value.Count.ShouldBe(1);
        (await GetLatestVersionAsync()).ShouldBe(0);

        // Act: Deactivate and re-activate the logViewGAgent.
        await Silo.DeactivateAsync(logViewGAgent);
        logViewGAgent = await Silo.CreateGrainAsync<LogViewAdaptorTestGAgent>(guid);

        // Act: Second event.
        await publishingGAgent.PublishEventAsync(new NaiveTestEvent
        {
            Greeting = "Second event"
        });

        // Assert.
        await TestHelper.WaitUntilAsync(_ => CheckCount(2));
        Silo.TestLogConsistentStorage.Storage.Count.ShouldBe(1);
        Silo.TestLogConsistentStorage.Storage.Last().Value.Count.ShouldBe(2);
        var logViewGAgentState = await logViewGAgent.GetStateAsync();
        await TestHelper.WaitUntilAsync(_ => CheckCount(logViewGAgentState, 2));
        logViewGAgentState.Content.Count.ShouldBe(2);
        (await GetLatestVersionAsync()).ShouldBe(1);

        // Act: Third event.
        await publishingGAgent.PublishEventAsync(new NaiveTestEvent
        {
            Greeting = "Third event"
        });

        await TestHelper.WaitUntilAsync(_ => CheckCount(3));
        (await GetLatestVersionAsync()).ShouldBe(2);
    }

    private async Task<bool> CheckCount(int expectedCount)
    {
        return Silo.TestLogConsistentStorage.Storage.Count == 1
               && Silo.TestLogConsistentStorage.Storage.Last().Value.Count == expectedCount;
    }

    private async Task<bool> CheckCount(LogViewAdaptorTestGState state, int expectedCount)
    {
        return state.Content.Count == expectedCount;
    }

    private async Task<int> GetLatestVersionAsync()
    {
        return await Silo.TestLogConsistentStorage.GetLastVersionAsync(string.Empty,
            GrainId.Create(string.Empty, string.Empty));
    }
}