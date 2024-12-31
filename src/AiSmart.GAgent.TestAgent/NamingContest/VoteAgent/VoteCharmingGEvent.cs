using AISmart.Agents;

namespace AiSmart.GAgent.TestAgent.NamingContest.VoteAgent;

[GenerateSerializer]
public class VoteCharmingGEvent:GEventBase
{
    [Id(0)] public List<Guid> GrainGuidList { get; set; }
}

[GenerateSerializer]
public class InitVoteCharmingGEvent:GEventBase
{
    [Id(0)] public List<Guid> GrainGuidList { get; set; }
    [Id(1)] public int TotalBatches { get; set; }
    
    [Id(2)] public int Round { get; set; }
   
}
