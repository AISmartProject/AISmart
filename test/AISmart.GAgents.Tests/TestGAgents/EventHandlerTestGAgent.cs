using AISmart.Agents;
using AISmart.GAgent.Core;
using AISmart.GAgents.Tests.TestEvents;
using Microsoft.Extensions.Logging;
using Orleans.Storage;

namespace AISmart.GAgents.Tests.TestGAgents;

[GenerateSerializer]
public class EventHandlerTestGAgentState : StateBase
{
    [Id(0)]  public List<string> Content { get; set; }
}

public class EventHandlerTestGEvent : GEventBase;

[GAgent]
public class EventHandlerTestGAgent : EventHandler<EventHandlerTestGAgentState, EventHandlerTestGEvent>
{
    public EventHandlerTestGAgent(ILogger logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("This GAgent is used for testing event handlers.");
    }

    // This method can be recognized as an event handler,
    // because the method name matches `HandleEventAsync`.
    public Task HandleEventAsync(NaiveTestEvent eventData)
    {
        AddContent(eventData.Greeting);
        return Task.CompletedTask;
    }

    [EventHandler]
    public Task ExecuteAsync(NaiveTestEvent eventData)
    {
        AddContent(eventData.Greeting);
        return Task.CompletedTask;
    }

    [AllEventHandler]
    public Task HandleEventAsync(EventWrapperBase eventData)
    {
        if (eventData is EventWrapper<EventBase> wrapper)
        {
            AddContent($"{wrapper.EventId}: {wrapper.Event.GetType()}");
        }

        return Task.CompletedTask;
    }

    private void AddContent(string content)
    {
        if (State.Content.IsNullOrEmpty())
        {
            State.Content = [];
        }

        State.Content.Add(content);
    }
}