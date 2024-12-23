using AISmart.Agents;
using AISmart.GAgent.Core;
using Microsoft.Extensions.Logging;
using Orleans.Storage;

namespace AISmart.GAgents.Tests.TestGAgents;

[GenerateSerializer]
public class SubscribeTestGAgentState : StateBase
{
    [Id(0)]  public Dictionary<Type, List<Type>> SubscriptionInfo { get; set; }
}

public class SubscribeTestGEvent : GEventBase;

[GAgent]
public class SubscribeTestGAgent : EventHandler<SubscribeTestGAgentState, SubscribeTestGEvent>
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
        if (State.SubscriptionInfo.IsNullOrEmpty())
        {
            State.SubscriptionInfo = eventData.Value;
        }
    }
}