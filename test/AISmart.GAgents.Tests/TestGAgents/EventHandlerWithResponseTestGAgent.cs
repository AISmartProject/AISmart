using AISmart.Agents;
using AISmart.GAgent.Core;
using AISmart.GAgents.Tests.TestEvents;
using Microsoft.Extensions.Logging;
using Orleans.Storage;

namespace AISmart.GAgents.Tests.TestGAgents;

[GenerateSerializer]
public class EventHandlerWithResponseTestGAgentState : StateBase
{
    [Id(0)]  public List<string> Content { get; set; }
}

public class EventHandlerWithResponseTestGEvent : GEventBase;

[GAgent]
public class
    EventHandlerWithResponseTestGAgent : GAgentBase<EventHandlerWithResponseTestGAgentState,
    EventHandlerWithResponseTestGEvent>
{
    public EventHandlerWithResponseTestGAgent(ILogger logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("This GAgent is used for testing event handler with response.");
    }

    [EventHandler]
    public async Task<NaiveTestEvent> ExecuteAsync(ResponseTestEvent responseTestEvent)
    {
        return new NaiveTestEvent
        {
            Greeting = responseTestEvent.Greeting
        };
    }
}