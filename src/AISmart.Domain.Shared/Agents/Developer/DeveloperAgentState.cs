using System.Collections.Generic;
using Orleans;

namespace AISmart.Agents.Developer;

[GenerateSerializer]
public class DeveloperAgentState
{
    [Id(0)]  public List<string> Content { get; set; }
}