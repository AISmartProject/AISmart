using System.Collections.Generic;
using AISmart.Application.Grains.Agents.MockA;
using AISmart.Application.Grains.Agents.MockB;
using Orleans;

namespace AISmart.Agents.MockB;

[GenerateSerializer]
public class MockBAgentState : StateBase
{
    [Id(0)] public List<string> ThreadIds { get; set; }
    [Id(1)] public int Number { get; set; }
    
    public void Apply(MockBAddNumberEvent @event)
    {
        Number += 1;
    }
}