using AISmart.Agent.GEvents;
using AISmart.Agents;

namespace AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;

[GenerateSerializer]
public class JudgeOverGEvent:EventBase
{
    [Id(0)] public string NamingQuestion { get; set; }
}

[GenerateSerializer]
public class JudgeVoteGEVent : EventBase
{
    [Id(0)] public Guid JudgeGrainId { get; set; }
    
    [Id(1)] public List<MicroAIMessage> History { get; set; } = new List<MicroAIMessage>();
}

[GenerateSerializer]
public class JudgeVoteResultGEvent : EventBase
{
    [Id(0)]
    public Guid JudgeGrainId { get; set; }
    [Id(1)]
    public String JudgeName { get; set; }
    [Id(2)]
    public string VoteName { get; set; }
    [Id(3)]
    public string Reason { get; set; }
}