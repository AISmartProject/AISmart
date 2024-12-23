using AISmart.Agents;
using AISmart.GAgent.Core;
using AISmart.GAgents.Tests.TestEvents;
using Microsoft.Extensions.Logging;
using Orleans.Storage;

namespace AISmart.GAgents.Tests.TestGAgents;

[GenerateSerializer]
public class FatalEventHandlerTestGAgentState
{
    [Id(0)]  public List<string> Content { get; set; }
}

public class FatalEventHandlerTestGEvent : GEventBase;

[GAgent]
public class FatalEventHandlerTestGAgent : EventHandler<EventHandlerTestGAgentState, EventHandlerTestGEvent>
{
    public FatalEventHandlerTestGAgent(ILogger logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("This GAgent is used for testing invalid event handlers.");
    }

    // This will be recognized as an event handler,
    // but will throw an exception because NotImplEventBaseTestEvent is not derived from EventBase.
    public Task<NotImplEventBaseTestEvent> HandleEventAsync(ResponseTestEvent eventData)
    {
        return Task.FromResult(new NotImplEventBaseTestEvent());
    }
    
    // This will be recognized as an event handler,
    // but will throw an exception because this method doesn't have response event.
    public Task HandleEventAsync(AnotherResponseTestEvent eventData)
    {
        return Task.CompletedTask;
    }
}