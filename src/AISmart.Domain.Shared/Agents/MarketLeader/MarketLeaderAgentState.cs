using System.Collections.Generic;
using Orleans;

namespace AISmart.Agents.MarketLeader;

[GenerateSerializer]
public class MarketLeaderAgentState : StateBase
{
    [Id(0)]  public List<string> Content { get; set; }
}