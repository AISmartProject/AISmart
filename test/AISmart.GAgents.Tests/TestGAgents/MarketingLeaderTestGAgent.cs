using AISmart.Agents;
using AISmart.GAgent.Autogen;
using AISmart.GAgent.Core;
using AISmart.GAgents.Tests.TestEvents;
using Microsoft.Extensions.Logging;

namespace AISmart.GAgents.Tests.TestGAgents;

public interface IMarketingLeaderTestGAgent : IGAgent
{
    
}

[GAgent]
public class MarketingLeaderTestGAgent : GAgentBase<NaiveTestGAgentState, NaiveTestGEvent>, IMarketingLeaderTestGAgent
{
    public MarketingLeaderTestGAgent(ILogger<MarketingLeaderTestGAgent> logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("This GAgent acts as a marketing leader.");
    }

    public async Task HandleEventAsync(NewDemandTestEvent eventData)
    {
        await PublishAsync(new WorkingOnTestEvent
        {
            Description = $"Working on `{eventData.Description}`",
        });
    }

    public async Task HandleEventAsync(NewFeatureCompletedTestEvent eventData)
    {
        await PublishAsync(new WorkingOnTestEvent
        {
            Description = $"Working completed: {eventData.PullRequestUrl}"
        });
    }
    
    public async Task HandleEventAsync(InvestorFeedbackTestEvent eventData)
    {
        if (State.Content.IsNullOrEmpty())
        {
            State.Content = [];
        }

        State.Content.Add($"Feedback from investor: {eventData.Content}");
    }
}