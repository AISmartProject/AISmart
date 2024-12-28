using AISmart.Agent.GEvents;
using AISmart.Agents;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using Nest;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

public class FirstTrafficState : StateBase
{
    [Id(0)] public List<Guid> CalledGrainIdList { get; set; } = new List<Guid>();
    [Id(1)] public List<Guid> CreativeList { get; set; } = new List<Guid>();
    [Id(2)] public Guid CurrentCreativeId { get; set; }
    [Id(3)] public string NamingContent { get; set; }
    [Id(4)] public string AgentName { get; set; }
    [Id(5)] public string Description { get; set; }
    [Id(6)] public int DebateRoundCount { get; set; }
    [Id(7)] public List<Guid> DebateList { get; set; }
    [Id(8)] public NamingContestStepEnum NamingStep { get; set; }

    [Id(8)] public List<Guid> JudgeAgentList { get; set; } = new List<Guid>();
    [Id(9)] public List<MicroAIMessage> ChatHistory { get; set; } = new List<MicroAIMessage>();

    public void Apply(TrafficCallSelectGrainidSEvent sEvent)
    {
        CurrentCreativeId = sEvent.GrainId;
    }

    public void Apply(TrafficNameStartSEvent @event)
    {
        NamingContent = @event.Content;
    }

    public void Apply(TrafficCreativeCompleteGEvent gEvent)
    {
        CalledGrainIdList.Add(gEvent.CompleteGrainId);
        CurrentCreativeId = Guid.Empty;
    }

    public void Apply(TrafficSetAgentSEvent @event)
    {
        AgentName = @event.AgentName;
        Description = @event.Description;
    }

    public void Apply(AddCreativeAgent @event)
    {
        if (CreativeList.Contains(@event.CreativeGrainId))
        {
            return;
        }

        CreativeList.Add(@event.CreativeGrainId);
    }

    public void Apply(ChangeNamingStepSEvent @event)
    {
        NamingStep = @event.Step;
    }

    public void Apply(SetDebateCountSEvent @event)
    {
        DebateRoundCount = @event.DebateCount;
    }

    public void Apply(ReduceDebateRoundSEvent @event)
    {
        DebateRoundCount -= 1;
    }

    public void Apply(AddChatHistorySEvent @event)
    {
        ChatHistory.Add(@event.ChatMessage);
    }

    public void Apply(AddJudgeSEvent @event)
    {
        this.JudgeAgentList.Add(@event.JudgeGrainId);
    }

    public void Apply(ClearCalledGrainsSEvent @event)
    {
        this.CalledGrainIdList.Clear();
    }
}