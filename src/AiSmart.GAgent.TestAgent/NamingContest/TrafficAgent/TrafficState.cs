
using AISmart.Agents;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

public class TrafficState : StateBase
{
    [Id(0)] public List<Guid> CalledCreativeList { get; set; } = new List<Guid>();
    [Id(1)] public Guid CurrentCreativeId { get; set; }
    [Id(2)] public string NamingContent { get; set; }
    [Id(3)] public string AgentName { get; set; }
    [Id(4)] public string Description { get; set; }

    public void Apply(TrafficCallCreativeSEvent sEvent)
    {
        CurrentCreativeId = sEvent.CreativeGrainId;
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
}