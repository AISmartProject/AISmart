using System.ComponentModel;
using AISmart.Agents.ImplementationAgent.Events;
using AISmart.Agents.MarketLeader;
using AISmart.Agents.MarketLeader.Events;
using AISmart.Events;
using AISmart.GAgent.Core;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.Application.Grains.Agents.MarketLeader;
[Description("Marketing department，I can handle tasks related to the marketing department.")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class MarketLeaderGAgent : GAgentBase<MarketLeaderAgentState, MarketLeaderGEvent>
{
    public MarketLeaderGAgent(ILogger<MarketLeaderGAgent> logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("An agent to inform other agents when a social event is published.");
    }

    public async Task<ImplementationEvent> HandleEventAsync(SocialEvent eventData)
    {
        Logger.LogInformation($"{GetType()} ExecuteAsync: Market Leader analyses content:{eventData.Content}");
        await PublishAsync(new SendMessageEvent
        {
            Message = "MarketLeaderGAgent Completed."
        });
        return new ImplementationEvent
        {
            Content = "Done"
        };
    }
}