using AISmart.Agents.Developer;
using AISmart.Agents.ImplementationAgent.Events;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.Application.Grains.Agents.Developer;

[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class DeveloperGAgent : GAgentBase<DeveloperAgentState, DeveloperGEvent>
{
    public DeveloperGAgent(ILogger<DeveloperGAgent> logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("An agent to inform other agents when a social event is published.");
    }

    public async Task<WorkCompleteEvent> HandleEventAsync(ImplementationEvent eventData)
    {
        Logger.LogInformation($"{GetType()} ExecuteAsync: DeveloperAgent analyses content:{eventData.Content}");
        if (State.Content.IsNullOrEmpty())
        {
            State.Content = [];
        }
        State.Content.Add(eventData.Content);
        return new WorkCompleteEvent
        {
            Content = eventData.Content
        };
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        GrainTracker.DeveloperAgents.Enqueue(this);
        await base.OnActivateAsync(cancellationToken);
    }
}