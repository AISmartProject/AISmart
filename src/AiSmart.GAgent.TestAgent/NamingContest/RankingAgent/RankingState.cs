
using AISmart.Agents;
using Nest;

namespace AiSmart.GAgent.TestAgent.NamingContest.RankingAgent;

public class RankingState: StateBase
{
    [Id(0)] public Dictionary<string, List<RankInfo>> RankDic { get; set; } = new Dictionary<string, List<RankInfo>>();

    public void Apply(RankingSEvent @event)
    {
        if (RankDic.TryGetValue(@event.Question, out var value) == false)
        {
            value = new List<RankInfo>();
            RankDic.Add(@event.Question,value);
        }
        
        value.Add(new RankInfo()
        {
            CreativeGrainId = @event.CreativeGrainId,
            Reply = @event.Reply,
            Score = @event.Score,
            CreativeName = @event.CreativeName
        });
        
        value.Sort((s1,s2)=> s1.Score.CompareTo(s2.Score));
    }

    public void Apply(RankingCleanSEvent rankingCleanSEvent)
    {
        RankDic.Remove(rankingCleanSEvent.Question);
    }
}

public class RankInfo
{
    [Id(0)] public Guid CreativeGrainId { get; set; }
    [Id(1)] public string Reply { get; set; }
    [Id(2)] public int Score { get; set; }
    [Id(3)] public string CreativeName { get; set; }
}
