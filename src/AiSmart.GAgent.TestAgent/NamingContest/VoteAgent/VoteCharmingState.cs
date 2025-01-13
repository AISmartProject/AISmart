using AISmart.Agents;

namespace AiSmart.GAgent.TestAgent.NamingContest.VoteAgent;

[GenerateSerializer]
public class VoteCharmingState : StateBase
{
    [Id(0)] public List<Guid> VoterIds { get; set; } = new List<Guid>();
    [Id(1)] public int TotalBatches { get; set; }
    [Id(2)] public int CurrentBatch { get; set; }
    [Id(3)] public int Round { get; set; }
    [Id(4)] public Dictionary<Guid, string> VoterIdTypeDictionary { get; set; } = new();
    [Id(5)] public List<Guid> GroupList { get; set; } = new List<Guid>();
    [Id(6)] public int TotalGroupCount { get; set; } = 0;
    [Id(7)] public int GroupHasVoteCount { get; set; } = 0;

    public void Apply(InitVoteCharmingGEvent @event)
    {
        VoterIds.AddRange(@event.GrainGuidList);
        TotalBatches = @event.TotalBatches;
        Round = @event.Round;
        VoterIdTypeDictionary = @event.GrainGuidTypeDictionary;
        GroupList = @event.GroupList;
        TotalGroupCount = @event.TotalGroupCount;
    }

    public void Apply(VoteCharmingGEvent @event)
    {
        VoterIds.RemoveAll(@event.GrainGuidList);
        foreach (var voterId in VoterIds)
        {
            VoterIdTypeDictionary.Remove(voterId);
        }

        CurrentBatch++;
    }

    public void Apply(GroupVoteCompleteSEvent @event)
    {
        foreach (var traffic in @event.VoteGroupList)
        {
            GroupList.Remove(traffic);
        }

        GroupHasVoteCount += 1;
    }
}