namespace AiSmart.GAgent.TestAgent.NamingContest.VoteAgent.Grains;

public interface IVoteAgentGrain :  IGrainWithGuidKey
{
    Task VoteAgentAsync(VoteCharmingEvent voteCharmingEvent);
    
    Task VoteAgentAsync(SingleVoteCharmingEvent singleVoteCharmingEvent);

}