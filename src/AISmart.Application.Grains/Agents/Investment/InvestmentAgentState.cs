namespace AISmart.Application.Grains.Agents.MarketLeader;

[GenerateSerializer]
public class InvestmentAgentState
{
    [Id(0)]  public List<string> Content { get; set; }
}