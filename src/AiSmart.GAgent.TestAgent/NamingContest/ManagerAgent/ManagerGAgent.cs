using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Events;
using AISmart.GAgent.Core;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.Grains;
using AISmart.Options;
using AISmart.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiSmart.GAgent.TestAgent.NamingContest.ManagerAgent;

public class ManagerGAgent : GAgentBase<ManagerAgentState, ManagerSEvent>, IManagerGAgent
{
    

    public ManagerGAgent( ILogger<ManagerGAgent> logger) : base(logger)
    {
    }
    

    public override Task<string> GetDescriptionAsync()
    {
        throw new NotImplementedException();
    }

    public async Task InitAgentsAsync(InitAgentMessageSEvent initAgentMessageSEvent)
    {
        RaiseEvent(initAgentMessageSEvent);
        await base.ConfirmEvents();
        
    }

    public async Task InitGroupInfoAsync(InitNetWorkMessageSEvent initNetWorkMessageSEvent,string groupAgentId )
    {
        RaiseEvent(initNetWorkMessageSEvent);
        await base.ConfirmEvents();
        
    }

    public async Task ClearAllAgentsAsync()
    {
        RaiseEvent(new ClearAllAgentMessageSEvent());
        RaiseEvent(new ClearAllNetWorkMessageSEvent());
        await base.ConfirmEvents();
    }
    
    public async Task<ManagerAgentState> GetManagerAgentStateAsync()
    {
        return State;
    }
}