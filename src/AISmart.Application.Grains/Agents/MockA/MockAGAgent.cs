using System.Diagnostics.CodeAnalysis;
using AISmart.Agents;
using AISmart.Agents.MockA;
using AISmart.Agents.MockA.Events;
using AISmart.Agents.MockB.Events;
using AISmart.GAgent.Core;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.Application.Grains.Agents.MockA;

[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MockAGAgent : GAgentBase<MockAAgentState, MockAGEvent>
{
    private readonly IMockAGAgentCount _mockAGAgentCount;

    public MockAGAgent(ILogger<MockAGAgent> logger, IMockAGAgentCount mockAGAgentCount) : base(logger)
    {
        _mockAGAgentCount = mockAGAgentCount;
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("An agent to inform other agents when an A thread is published.");
    }

    [EventHandler]
    public async Task HandleEventAsync(MockAThreadCreatedEvent eventData)
    {
        Logger.LogInformation($"{GetType()} ExecuteAsync: AAgent analyses content: {eventData.Content}");

        if (State.ThreadIds == null)
        {
            State.ThreadIds = new List<string>();
        }

        State.ThreadIds.Add(eventData.Id);

        var publishEvent = new MockBThreadCreatedEvent
        {
            Content = $"A Thread {eventData.Content} has been published."
        };

        await PublishAsync(publishEvent);
        await PublishAsync(new RequestAllSubscriptionsEvent());

        _mockAGAgentCount.AGAgentCount();
    }
}