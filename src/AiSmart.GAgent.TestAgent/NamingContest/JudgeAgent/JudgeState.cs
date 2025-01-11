using AISmart.Agent.GEvents;

namespace AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;

[GenerateSerializer]
public class JudgeState : StateBase
{
    [Id(1)] public string AgentName { get; set; }
    [Id(2)] public string AgentResponsibility { get; set; }
    [Id(3)] public Queue<MicroAIMessage> RecentMessages = new Queue<MicroAIMessage>();
    [Id(4)] public Guid CloneJudgeId = Guid.Empty;
    
    public void Apply(AISetAgentMessageGEvent aiSetAgentMessageGEvent)
    {
        AgentName = aiSetAgentMessageGEvent.AgentName;
        AgentResponsibility = aiSetAgentMessageGEvent.AgentResponsibility;
    }
    
    public void Apply(AIReceiveMessageGEvent aiReceiveMessageGEvent)
    {
        AddMessage(aiReceiveMessageGEvent.Message);
    }
    
    public void Apply(AIReplyMessageGEvent aiReplyMessageGEvent)
    {
        AddMessage(aiReplyMessageGEvent.Message);
    }

    public void Apply(AIClearMessageGEvent clearMessageGEvent)
    {
        RecentMessages = new Queue<MicroAIMessage>();
    }

    public void Apply(JudgeCloneSEvent @event)
    {
        CloneJudgeId = @event.JudgeGrainId;
    }
    
    void AddMessage(MicroAIMessage message)
    {
        if (RecentMessages.Count == 10)
        {
            RecentMessages.Dequeue(); 
        }
        RecentMessages.Enqueue(message); 
    }
    
}