using System.ComponentModel;
using AISmart.Agents.Developer;
using AISmart.Agents.ImplementationAgent.Events;
using AISmart.Events;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.Application.Grains.Agents.Developer;
[Description("R&D department, and I can handle development-related tasks.")]
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
        await PublishAsync(new SendMessageEvent
        {
            Message = "DeveloperGAgent Completed."
        });
        return new WorkCompleteEvent
        {
            Content = "Done"
        };
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        GrainTracker.DeveloperAgents.Enqueue(this);
        await base.OnActivateAsync(cancellationToken);
    }
}