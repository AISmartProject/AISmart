using AISmart.Agents;
using AISmart.Agents.Publisher;
using AISmart.Dapr;
using AISmart.GAgent.Core;
using AISmart.Sender;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.Application.Grains.Agents.Publisher;

[GenerateSerializer]
public class PublishingAgentState : StateBase
{
}

[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class PublishingGAgent : ContextGAgentBase<PublishingAgentState, PublishingGEvent>, IPublishingGAgent
{
    public PublishingGAgent(ILogger<PublishingGAgent> logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("Agent to be used for publishing new events.");
    }

    public async Task PublishEventAsync<T>(T @event) where T : EventBase
    {
        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        Logger.LogInformation($"PublishingAgent publish {@event}");
        await PublishAsync(@event);
    }
}