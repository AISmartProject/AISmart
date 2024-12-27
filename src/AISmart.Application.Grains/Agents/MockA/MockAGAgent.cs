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
public class MockAGAgent : GAgentBase<MockAAgentState, MockAGEvent>, IMockAGAgentCount
{
    public MockAGAgent(ILogger<MockAGAgent> logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("An agent to inform other agents when an A thread is published.");
    }

    [EventHandler]
    public async Task HandleEventAsync(MockAThreadCreatedEvent eventData)
    {
        Logger.LogInformation($"{GetType()} ExecuteAsync: AAgent analyses content: {eventData.Content}");

        RaiseEvent(new MockAAddNumberEvent());

        if (State.ThreadIds == null)
        {
            State.ThreadIds = new List<string>();
        }

        State.ThreadIds.Add(eventData.Id);

        var publishEvent = new MockBThreadCreatedEvent
        {
            Id = $"mock_C_thread_id",
            Content = $"Call mockBGAgent"
        };

        await PublishAsync(publishEvent);
    }

    public Task<int> GetMockAGAgentCount()
    {
        return Task.FromResult(State.Number);
    }
}