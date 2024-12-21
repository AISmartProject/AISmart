using System.Collections.Generic;
using Orleans;

namespace AISmart.Agents.Investment;

[GenerateSerializer]
public class InvestmentAgentState
{
    [Id(0)]  public List<string> Content { get; set; }
}