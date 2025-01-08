using AISmart.Agents.MarketLeader.Events;
using AISmart.Agents.X.Events;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.Application.Grains.Agents.X;

[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class XAgent : GAgent<XAgentState, XThreadCreatedEvent>
{
    public XAgent(ILogger<XAgent> logger, IClusterClient clusterClient) : base(logger, clusterClient)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("An agent to inform other agents when a X thread is published.");
    }

    private Task TryExecuteAsync(XThreadCreatedEvent eventData)
    {
        Logger.LogInformation("TryExecuteAsync: X Thread {XContent}", eventData.Content);
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(XThreadCreatedEvent eventData)
    {
        Logger.LogInformation($"{GetType()} ExecuteAsync: XAgent analyses content:{eventData.Content}");
        if (State.ThreadIds.IsNullOrEmpty())
        {
            State.ThreadIds = [];
        }

        State.ThreadIds.Add(eventData.Id);

        var publishEvent = new SocialEvent
        {
            Content = $"X Thread {eventData.Content} has been published."
        };
        await PublishAsync(publishEvent);
    }

    protected override Task CompleteAsync(XThreadCreatedEvent eventData)
    {
        Logger.LogInformation("CompleteAsync: X Thread {XContent}", eventData.Content);

        return Task.CompletedTask;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        GrainTracker.XAgents.Enqueue(this);
        await base.OnActivateAsync(cancellationToken);
        await SubscribeAsync<XThreadCreatedEvent>(ExecuteAsync);
    }
}