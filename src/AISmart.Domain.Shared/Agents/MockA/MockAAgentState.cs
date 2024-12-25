using System.Collections.Generic;
using Orleans;

namespace AISmart.Agents.MockA;

[GenerateSerializer]
public class MockAAgentState : StateBase
{
    [Id(0)] public List<string> ThreadIds { get; set; }
    [Id(1)] public int Number { get; set; }
}