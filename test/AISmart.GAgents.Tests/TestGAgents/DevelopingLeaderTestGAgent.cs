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
        await PublishAsync(new DevelopTaskTestEvent
        {
            Description = $"This is the demand for the task: {eventData.Description}"
        });
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
            await PublishAsync(new NewFeatureCompletedTestEvent
            {
                PullRequestUrl = string.Join("\n", State.Content)
            });
        }
    }
}