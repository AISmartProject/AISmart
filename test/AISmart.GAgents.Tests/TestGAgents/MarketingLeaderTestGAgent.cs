using AISmart.GAgent.Core;
using AISmart.GAgents.Tests.TestEvents;
using Microsoft.Extensions.Logging;

namespace AISmart.GAgents.Tests.TestGAgents;

[GAgent]
public class MarketingLeaderTestGAgent : GAgentBase<NaiveTestGAgentState, NaiveTestGEvent>
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
            Description = eventData.Description
        });
    }

    public async Task HandleEventAsync(NewFeatureCompletedTestEvent eventData)
    {
        var newEvent = new WorkingOnTestEvent
        {
            Description = eventData.PullRequestUrl
        };
        newEvent.SetRootStreamIdList(eventData.GetRootStreamIdList());
        await PublishAsync(newEvent);
    }
}