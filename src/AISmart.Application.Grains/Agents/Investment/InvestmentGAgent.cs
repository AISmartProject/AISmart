using System.ComponentModel;
using AISmart.Agents;
using AISmart.Agents.ImplementationAgent.Events;
using AISmart.Agents.Investment;
using AISmart.Events;
using AISmart.GAgent.Core;
using Microsoft.Extensions.Logging;
using Orleans.Providers;
using Orleans.Storage;

namespace AISmart.Application.Grains.Agents.Investment;

[Description("Investment department,")]
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
    public async Task<WorkCompleteEvent> HandleEventAsync(InvestmentEvent eventData)
    {
        if (State.Content.IsNullOrEmpty())
        {
            State.Content = [];
        }

        State.Content.Add(eventData.Content);
        await PublishAsync(new SendMessageEvent
        {
            Message = "InvestmentGAgent Completed."
        });
        Logger.LogInformation($"{GetType()} ExecuteAsync: InvestmentAgent analyses content:{eventData.Content}");

        return new WorkCompleteEvent
        {
            Content = "Done"
        };
    }
}