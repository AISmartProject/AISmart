using AISmart.Agents;
using AISmart.Agents.Publisher;
using AISmart.Sender;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.Application.Grains.Agents.Publisher;

[GenerateSerializer]
public class PublishingAgentState
{
}

[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class PublishingGAgent : GAgentBase<PublishingAgentState, PublishingGEvent>, IPublishingAgent
{
    public PublishingGAgent(ILogger<PublishingGAgent> logger, IClusterClient clusterClient) : base(logger, clusterClient)
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