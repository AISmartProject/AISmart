using System.Diagnostics.CodeAnalysis;
using AISmart.Agents;
using AISmart.Agents.MockB;
using AISmart.Agents.MockB.Events;
using AISmart.GAgent.Core;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.Application.Grains.Agents.MockB;

[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MockBGAgent : GAgentBase<MockBAgentState, MockBGEvent>, IMockBGAgentCount
{
    public MockBGAgent(ILogger<MockBGAgent> logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("An agent to inform other agents when a A thread is published.");
    }

    private Task TryExecuteAsync(MockBThreadCreatedEvent eventData)
    {
        Logger.LogInformation("TryExecuteAsync: A Thread {AContent}", eventData.Content);
        return Task.CompletedTask;
    }

    [EventHandler]
    public async Task HandleEventAsync(MockBThreadCreatedEvent eventData)
    {
        Logger.LogInformation($"{GetType()} ExecuteAsync: BAgent analyses content: {eventData.Content}");

        RaiseEvent(new MockBAddNumberEvent());

        if (State.ThreadIds.IsNullOrEmpty())
        {
            State.ThreadIds = new List<string>();
        }

        State.ThreadIds.Add(eventData.Id);

        var publishEvent = new MockCThreadCreatedEvent
        {
            Id = $"mock_C_thread_id",
            Content = $"Call mockCGAgent"
        };

        await PublishAsync(publishEvent);
    }

    public Task<int> GetMockBGAgentCount()
    {
        return Task.FromResult(State.Number);
    }
}