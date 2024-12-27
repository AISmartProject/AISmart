using AISmart.Agents;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

public class FirstTrafficState : StateBase
{
    [Id(0)] public List<Guid> CalledCreativeList { get; set; } = new List<Guid>();
    [Id(1)] public List<Guid> CreativeList { get; set; } = new List<Guid>();

    [Id(2)] public Guid CurrentCreativeId { get; set; }
    [Id(3)] public string NamingContent { get; set; }
    [Id(4)] public string AgentName { get; set; }
    [Id(5)] public string Description { get; set; }
    
    [Id(6)] public int DebateStageCount { get; set; }
    
    

    public void Apply(TrafficCallSelectCreativeSEvent sEvent)
    {
        CurrentCreativeId = sEvent.CreativeGrainId;
    }

    public void Apply(TrafficNameStartSEvent @event)
    {
        NamingContent = @event.Content;
    }
    
    public void Apply(TrafficCreativeCompleteGEvent gEvent)
    {
        CalledCreativeList.Add(gEvent.CompleteGrainId);
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
}