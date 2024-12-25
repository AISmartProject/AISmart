using System.Diagnostics.CodeAnalysis;
using AISmart.Agents;
using AISmart.Agents.MockB.Events;
using AISmart.Agents.MockC;
using AISmart.GAgent.Core;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.Application.Grains.Agents.MockC;

[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MockCGAgent : GAgentBase<MockCAgentState, MockCGEvent>
{
    private readonly IMockCGAgentCount _mockCGAgentCount;

    public MockCGAgent(ILogger<MockCGAgent> logger, IMockCGAgentCount mockCGAgentCount) : base(logger)
    {
        _mockCGAgentCount = mockCGAgentCount;
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("An agent to inform other agents when a A thread is published.");
    }

    private Task TryExecuteAsync(MockCThreadCreatedEvent eventData)
    {
        Logger.LogInformation("TryExecuteAsync: A Thread {AContent}", eventData.Content);
        return Task.CompletedTask;
    }

    [EventHandler]
    public async Task HandleEventAsync(MockCThreadCreatedEvent eventData)
    {
        Console.WriteLine($"{GetType()} ExecuteAsync: CAgent analyses content: {eventData.Content}");
        await TryExecuteAsync(eventData);

        _mockCGAgentCount.CGAgentCount();
    }
}