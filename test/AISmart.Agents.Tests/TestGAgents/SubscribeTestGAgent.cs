using AISmart.Agents;
using AISmart.Application.Grains;
using Microsoft.Extensions.Logging;

namespace AISmart.Grains.Tests.TestGAgents;

[GenerateSerializer]
public class SubscribeTestGAgentState
{
    [Id(0)]  public Dictionary<Type, List<Type>> SubscriptionInfo { get; set; }
}

public class SubscribeTestGEvent : GEventBase;

[GAgent]
public class SubscribeTestGAgent : GAgentBase<SubscribeTestGAgentState, SubscribeTestGEvent>
{
    public SubscribeTestGAgent(ILogger logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("This GAgent is used to test implementation of GetAllSubscribedEventsAsync.");
    }
    
    public async Task HandleEventAsync(SubscribedEventListEvent eventData)
    {
        State.SubscriptionInfo = eventData.Value;
    }
}