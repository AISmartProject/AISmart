using AISmart.Agents;
using AISmart.GAgent.Autogen.EventSourcingEvent;
using Nest;
using Volo.Abp.Guids;

namespace AiSmart.GAgent.TestAgent.NamingContest.Common;

[GenerateSerializer]
public class NamingLogEvent : EventBase
{
   [Id(0)] public NamingContestStepEnum Step { get; set; }
   [Id(1)] public NamingRoleType RoleType { get; set; }
   [Id(2)] public Guid AgentId { get; set; } = Guid.Empty;
   [Id(3)] public string AgentName { get; set; }
   [Id(4)] public string Content { get; set; }
   [Id(5)] public long Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); 

    public NamingLogEvent(NamingContestStepEnum step, Guid agentId, NamingRoleType roleType = NamingRoleType.None,
        string agentName = "", string content = "")
    {
        Step = step;
        AgentId = agentId;
        RoleType = roleType;
        AgentName = agentName;
        Content = content;
    }
}

[GenerateSerializer]
public enum NamingContestStepEnum
{
    NamingStart = 0,
    Naming = 1,
    DebateStart = 2,
    Debate = 3,
    DiscussionStart = 4,
    Discussion = 5,
    DiscussionSummary = 6,
    JudgeVoteStart = 7,
    JudgeVote = 8,
    JudgeStartScore= 9,
    JudgeScore = 10,
    JudgeStartAsking = 11,
    JudgeAsking = 12,
    Complete = 13,
}

[GenerateSerializer]
public enum NamingRoleType
{
    None = 0,
    Contestant = 1,
    Judge = 2,
}