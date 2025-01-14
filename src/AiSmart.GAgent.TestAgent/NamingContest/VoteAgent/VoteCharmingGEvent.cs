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
    [Id(3)] public Dictionary<Guid, string> GrainGuidTypeDictionary { get; set; }
    [Id(4)] public List<Guid> GroupList { get; set; } = new List<Guid>();
}

[GenerateSerializer]
public class GroupVoteCompleteSEvent:GEventBase
{
    [Id(0)] public List<Guid> VoteGroupList { get; set; }
}


[GenerateSerializer]
public class GroupHasVoteSEvent:GEventBase
{
}