using AISmart.Agents;
using AISmart.Application.Grains;
using Microsoft.Extensions.Logging;

namespace AISmart.Grains.Tests.TestGAgents;

[GenerateSerializer]
public class NaiveTestGAgentState
{
    [Id(0)]  public List<string> Content { get; set; }
}

public class NaiveTestGEvent : GEventBase
{
    
}

[GAgent]
public class NaiveTestGAgent : GAgentBase<NaiveTestGAgentState, NaiveTestGEvent>
{
    public NaiveTestGAgent(ILogger logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("This is a naive test GAgent");
    }
}