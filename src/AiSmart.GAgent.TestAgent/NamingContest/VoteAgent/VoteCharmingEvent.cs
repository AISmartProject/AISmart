using AISmart.Agent.GEvents;
using AISmart.Agents;

namespace AiSmart.GAgent.TestAgent.NamingContest.VoteAgent;

[GenerateSerializer]
public class VoteCharmingEvent:EventBase
{
    [Id(0)] public Dictionary<Guid, List<MicroAIMessage>> VoteMessage { get; set; } = new();
}

[GenerateSerializer]
public class InitVoteCharmingEvent:EventBase
{
    [Id(0)] public List<Guid> GrainGuidList { get; set; } = new();
    [Id(1)] public int TotalBatches { get; set; }
    [Id(2)] public int Round { get; set; }
}

[GenerateSerializer]
public class VoteCharmingCompleteEvent:EventBase
{
    [Id(0)] public Guid Winner { get; set; }
    [Id(1)] public Guid VoterId { get; set; }
    [Id(2)] public int Round { get; set; }
}
