using AISmart.Agent.GEvents;
using AISmart.Agents;

namespace AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;

public class CreativeState:StateBase
{
    [Id(1)]  public  string AgentName { get; set; }
    [Id(2)]  public  string AgentResponsibility{ get; set; }
    [Id(3)] public Queue<MicroAIMessage> RecentMessages = new Queue<MicroAIMessage>();
    [Id(4)] public string Naming { get; set; }

    public void Apply(AddHistoryChatSEvent @event)
    {
        RecentMessages.Enqueue(@event.Message);
    }

    public void Apply(ClearHistoryChatSEvent @event)
    {
        RecentMessages.Clear();
    }

    public void Apply(SetNamingSEvent @event)
    {
        Naming = @event.Naming;
    }

    public void Apply(SetAgentInfoSEvent @event)
    {
        AgentName = @event.AgentName;
        AgentResponsibility = @event.Description;
    }
}