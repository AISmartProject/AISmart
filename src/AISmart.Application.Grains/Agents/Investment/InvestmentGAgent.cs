using AISmart.Agents;
using AISmart.Agents.ImplementationAgent.Events;
using AISmart.Agents.Investment;
using AISmart.Dapr;
using Microsoft.Extensions.Logging;
using Orleans.Providers;
using Orleans.Streams;

namespace AISmart.Application.Grains.Agents.Investment;

[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class InvestmentGAgent : GAgentBase<InvestmentAgentState, InvestmentGEvent>, IInvestmentStateGAgent<InvestmentAgentState>
{
    public InvestmentGAgent(ILogger<InvestmentGAgent> logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("An agent to inform other agents when a social event is published.");
    }

    public Task<InvestmentAgentState> GetStateAsync()
    {
        return Task.FromResult(State);
    }

    [EventHandler]
    public async Task<WorkCompleteEvent> HandleEventAsync(ImplementationEvent eventData)
    {
        if (State.Content.IsNullOrEmpty())
        {
            State.Content = [];
        }

        State.Content.Add(eventData.Content);
        Logger.LogInformation($"{GetType()} ExecuteAsync: InvestmentAgent analyses content:{eventData.Content}");

        return new WorkCompleteEvent
        {
            Content = eventData.Content
        };
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        GrainTracker.InvestmentAgents.Enqueue(this);
        await base.OnActivateAsync(cancellationToken);
    }
}