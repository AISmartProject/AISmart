using System.Collections.Generic;
using Orleans;

namespace AISmart.Agents.MarketLeader;

[GenerateSerializer]
public class MarketLeaderAgentState
{
    [Id(0)]  public List<string> Content { get; set; }
}