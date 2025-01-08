using AISmart.Agents;
using AISmart.GAgent.Core;
using AISmart.GAgents.Tests.TestEvents;
using Microsoft.Extensions.Logging;

namespace AISmart.GAgents.Tests.TestGAgents;

public interface IInvestorTestGAgent: IGAgent
{
    
}

[GenerateSerializer]
public class InvestorTestGAgentState : NaiveTestGAgentState
{
    
}

[GAgent]
public class InvestorTestGAgent : GAgentBase<InvestorTestGAgentState, NaiveTestGEvent>, IInvestorTestGAgent
{
    public InvestorTestGAgent(ILogger<InvestorTestGAgent> logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("This GAgent acts as a investor.");
    }

    public async Task HandleEventAsync(WorkingOnTestEvent eventData)
    {
        if (State.Content.IsNullOrEmpty())
        {
            State.Content = [];
        }

        State.Content.Add(eventData.Description);

        await PublishAsync(new InvestorFeedbackTestEvent
        {
            Content = $"This is the feedback for the event: {eventData.Description}"
        });
    }
}