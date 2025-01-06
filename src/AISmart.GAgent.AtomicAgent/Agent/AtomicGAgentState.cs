using AISmart.Agents;
using AISmart.GAgent.AtomicAgent.Agent.GEvents;
using AISmart.GAgent.AtomicAgent.Models;


namespace AISmart.GAgent.AtomicAgent.Agent;

public class AtomicGAgentState : StateBase
{
    [Id(0)] public Guid Id { get; set; }
    [Id(1)] public string UserAddress { get; set; }
    [Id(2)] public AgentData? Properties { get; set; }
    
    public void Apply(CreateAgentGEvent createAgentGEvent)
    {
        Id = createAgentGEvent.Id;
        Properties = createAgentGEvent.Data;
        UserAddress = createAgentGEvent.UserAddress;
    }
    
    public void Apply(UpdateAgentGEvent updateAgentGEvent)
    {
        Properties = updateAgentGEvent.Data;
    }
    
    public void Apply(DeleteAgentGEvent deleteAgentGEvent)
    {
        UserAddress = "";
        Properties = null;
    }
}