using AISmart.Agents;

namespace AiSmart.GAgent.TestAgent.NamingContest.RankingAgent;

[GenerateSerializer]
public class RankingGEvent:EventBase
{
    [Id(0)] public Guid CreativeGrainId { get; set; }
    [Id(1)] public string CreativeName { get; set; }
    [Id(2)] public int Score { get; set; }
    [Id(3)] public string Question { get; set; }
    [Id(4)] public string Reply { get; set; }
}