using AISmart.Agents;
using AISmart.GAgent.Core;
using AISmart.GAgents.Tests.TestEvents;
using Microsoft.Extensions.Logging;
using Orleans.Storage;

namespace AISmart.GAgents.Tests.TestGAgents;

[GenerateSerializer]
public class BadEventHandlerTestGAgentState
{
    [Id(0)]  public List<string> Content { get; set; }
}

public class BadEventHandlerTestGEvent : GEventBase;

[GAgent]
public class BadEventHandlerTestGAgent : GAgentBase<EventHandlerTestGAgentState, EventHandlerTestGEvent>
{
    public BadEventHandlerTestGAgent(ILogger logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("This GAgent is used for testing invalid event handlers.");
    }

    // This won't be recognized as an event handler,
    // because the method name doesn't match `HandleEventAsync`,
    // and doesn't have `EventHandler` attribute.
    public Task ExecuteAsync(NaiveTestEvent eventData)
    {
        return Task.CompletedTask;
    }

    // This won't be recognized as an event handler,
    // because the parameter is not EventWrapperBase.
    [AllEventHandler]
    public Task HandleAsync(NaiveTestEvent eventData)
    {
        return Task.CompletedTask;
    }

    // This won't be recognized as an event handler,
    // because IncorrectTestEvent is not inherit from EventBase.
    [EventHandler]
    public Task HandleEventAsync(IncorrectTestEvent eventData)
    {
        return Task.CompletedTask;
    }
}