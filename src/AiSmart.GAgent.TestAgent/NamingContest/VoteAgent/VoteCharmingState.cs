using AISmart.Agents;

namespace AiSmart.GAgent.TestAgent.NamingContest.VoteAgent;

[GenerateSerializer]
public class VoteCharmingState: StateBase
{
    [Id(0)] public List<Guid> VoterIds { get; set; } = new List<Guid>();
    [Id(1)] public int TotalBatches { get; set; }
    [Id(2)] public int CurrentBatch { get; set; }
    
    [Id(3)] public int Round { get; set; }
    

    public void Apply(InitVoteCharmingGEvent @event)
    {
        VoterIds.AddRange(@event.GrainGuidList);
        TotalBatches = @event.TotalBatches;
        Round = @event.Round;
    }

    public void Apply(VoteCharmingGEvent @event)
    {
        VoterIds.RemoveAll(@event.GrainGuidList);
        CurrentBatch--;
    }
   
}

public class RankInfo
{
    [Id(0)] public Guid CreativeGrainId { get; set; }
    [Id(1)] public string Reply { get; set; }
    [Id(2)] public int Score { get; set; }
    [Id(3)] public string CreativeName { get; set; }
}
