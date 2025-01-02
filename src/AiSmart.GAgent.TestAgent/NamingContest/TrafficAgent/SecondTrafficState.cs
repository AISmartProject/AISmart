using AISmart.Agent.GEvents;
using AISmart.Agents;
using AiSmart.GAgent.TestAgent.NamingContest.Common;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

public class SecondTrafficState : StateBase
{
    [Id(0)] public List<Guid> CalledGrainIdList { get; set; } = new List<Guid>();
    [Id(1)] public List<CreativeInfo> CreativeList { get; set; } = new List<CreativeInfo>();
    [Id(2)] public Guid CurrentGrainId { get; set; }
    [Id(3)] public string NamingContent { get; set; }
    [Id(4)] public List<Guid> JudgeAgentList { get; set; } = new List<Guid>();
    [Id(5)] public int AskJudgeCount { get; set; }
    [Id(6)] public List<Guid> AskingJudges { get; set; } = new List<Guid>();
    [Id(7)] public List<MicroAIMessage> ChatHistory { get; set; } = new List<MicroAIMessage>();
    [Id(8)] public int DiscussionCount { get; set; }

    public void Apply(TrafficCallSelectGrainIdSEvent sEvent)
    {
        CurrentGrainId = sEvent.GrainId;
    }

    public void Apply(TrafficNameStartSEvent @event)
    {
        NamingContent = @event.Content;
    }

    public void Apply(TrafficGrainCompleteGEvent gEvent)
    {
        CalledGrainIdList.Add(gEvent.CompleteGrainId);
        CurrentGrainId = Guid.Empty;
    }

    public void Apply(AddCreativeAgent @event)
    {
        if (CreativeList.Exists(e => e.CreativeGrainId == @event.CreativeGrainId))
        {
            return;
        }

        CreativeList.Add(new CreativeInfo()
            { CreativeName = @event.CreativeName, CreativeGrainId = @event.CreativeGrainId, Naming = @event.Naming });
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

    public void Apply(CreativeNamingSEvent @event)
    {
        var creativeInfo = this.CreativeList.FirstOrDefault(f => f.CreativeGrainId == @event.CreativeId);
        if (creativeInfo != null)
        {
            creativeInfo.Naming = @event.Naming;
        }
    }

    public void Apply(SetAskingJudgeSEvent @event)
    {
        AskJudgeCount = @event.AskingJudgeCount;
    }

    public void Apply(AddAskingJudgeSEvent @event)
    {
        AskingJudges.Add(@event.AskingJudgeGrainId);
    }

    public void Apply(SetDiscussionSEvent @event)
    {
        DiscussionCount = @event.DiscussionCount;
    }
}