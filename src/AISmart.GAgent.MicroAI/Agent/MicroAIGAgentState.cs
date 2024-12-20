using System;
using System.Collections.Generic;
using AISmart.Agent.GEvents;
using Orleans;

namespace AISmart.Agent;

public class MicroAIGAgentState 
{
    [Id(0)]  public  Guid Id { get; set; }
    [Id(1)]  public  string AgentName { get; set; }
    [Id(2)]  public  string AgentResponsibility{ get; set; }
    [Id(3)] public Queue<MicroAIMessage> RecentMessages = new Queue<MicroAIMessage>();

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
    void AddMessage(MicroAIMessage message)
    {
        if (RecentMessages.Count == 10)
        {
            RecentMessages.Dequeue(); 
        }
        RecentMessages.Enqueue(message); 
    }

}

