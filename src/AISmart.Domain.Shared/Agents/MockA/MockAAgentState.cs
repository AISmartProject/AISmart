using System.Collections.Generic;
using AISmart.Application.Grains.Agents.MockA;
using Orleans;

namespace AISmart.Agents.MockA;

[GenerateSerializer]
public class MockAAgentState : StateBase
{
    [Id(0)] public List<string> ThreadIds { get; set; }
    [Id(1)] public int Number { get; set; }

    public void Apply(MockAAddNumberEvent @event)
    {
        Number += 1;
    }
}