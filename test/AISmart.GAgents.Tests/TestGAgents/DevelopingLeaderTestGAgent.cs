using AISmart.GAgent.Core;
using AISmart.GAgents.Tests.TestEvents;
using Microsoft.Extensions.Logging;

namespace AISmart.GAgents.Tests.TestGAgents;

[GAgent]
public class DevelopingLeaderTestGAgent : GAgentBase<NaiveTestGAgentState, NaiveTestGEvent>
{
    public DevelopingLeaderTestGAgent(ILogger<DevelopingLeaderTestGAgent> logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("This GAgent acts as a developing leader.");
    }
    
    public async Task HandleEventAsync(NewDemandTestEvent eventData)
    {
        var newEvent = new DevelopTaskTestEvent
        {
            Description = $"This is the demand for the task: {eventData.Description}"
        };
        // TODO: This should be done by CorrelationId.
        newEvent.SetRootStreamIdList(eventData.GetRootStreamIdList());
        await PublishEventDownwardsAsync(newEvent);
    }

    public async Task HandleEventAsync(NewFeatureCompletedTestEvent eventData)
    {
        if (State.Content.IsNullOrEmpty())
        {
            State.Content = [];
        }

        State.Content.Add(eventData.PullRequestUrl);

        if (State.Content.Count == 3)
        {
            var newEvent = new NewFeatureCompletedTestEvent
            {
                PullRequestUrl = string.Join("\n", State.Content)
            };
            newEvent.SetRootStreamIdList(eventData.GetRootStreamIdList());
            await PublishAsync(newEvent);
        }
    }
}