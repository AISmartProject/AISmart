using System.Collections.Generic;
using Orleans;

namespace AISmart.Agents.X;

[GenerateSerializer]
public class XAgentState
{
    [Id(0)] public List<string> ThreadIds { get; set; }
}