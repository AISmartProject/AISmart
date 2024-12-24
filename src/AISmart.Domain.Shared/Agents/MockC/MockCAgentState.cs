using System.Collections.Generic;
using AISmart.Application.Grains.Agents.MockC;
using Orleans;

namespace AISmart.Agents.MockC;

[GenerateSerializer]
public class MockCAgentState : StateBase
{
    [Id(0)] public List<string> ThreadIds { get; set; }
    [Id(1)] public int Number { get; set; }
    
    public void Apply(MockCAddNumberEvent @event)
    {
        Number += 1;
    }
}