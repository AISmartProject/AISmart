using AISmart.Agents;
using AISmart.GAgent.Core;
using AISmart.GAgents.Tests.TestEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISmart.GAgents.Tests.TestGAgents;

[GAgent]
public class LogViewAdaptorTestGAgent
    : GAgentBase<LogViewAdaptorTestGState, LogViewAdaptorTestGEvent>
{
    public LogViewAdaptorTestGAgent(ILogger logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        throw new NotImplementedException();
    }

    public async Task HandleEventAsync(NaiveTestEvent eventData)
    {
        if (State.Content.IsNullOrEmpty())
        {
            State.Content = new Dictionary<Guid, LogViewAdaptorTestGEvent>();
        }
        RaiseEvent(new LogViewAdaptorTestGEvent
        {
            Greeting = eventData.Greeting
        });
        await ConfirmEvents();
    }
}

[GenerateSerializer]
public class LogViewAdaptorTestGState : StateBase
{
    [Id(0)] public Guid Id { get; set; }

    [Id(1)] public Dictionary<Guid, LogViewAdaptorTestGEvent> Content { get; set; } = new();

    public void Apply(LogViewAdaptorTestGEvent gEvent)
    {
        if (Id == Guid.Empty)
        {
            Id = Guid.NewGuid();
        }

        Content[gEvent.Id] = gEvent;
    }
}

[GenerateSerializer]
public class LogViewAdaptorTestGEvent : GEventBase
{
    [Id(0)] public Guid Id { get; set; } = Guid.NewGuid();
    [Id(2)] public string Greeting { get; set; }
}