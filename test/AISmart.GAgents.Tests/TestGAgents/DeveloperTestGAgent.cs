using AISmart.GAgent.Core;
using AISmart.GAgents.Tests.TestEvents;
using Microsoft.Extensions.Logging;

namespace AISmart.GAgents.Tests.TestGAgents;

[GAgent]
public class DeveloperTestGAgent : GAgentBase<NaiveTestGAgentState, NaiveTestGEvent>
{
    public DeveloperTestGAgent(ILogger<MarketingLeaderTestGAgent> logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("This GAgent acts as a developer.");
    }

    public async Task<NewFeatureCompletedTestEvent> HandleEventAsync(DevelopTaskTestEvent eventData)
    {
        if (State.Content.IsNullOrEmpty())
        {
            State.Content = [];
        }

        State.Content.Add(eventData.Description);

        return new NewFeatureCompletedTestEvent
        {
            PullRequestUrl = $"PR for {eventData.Description}"
        };
    }
}