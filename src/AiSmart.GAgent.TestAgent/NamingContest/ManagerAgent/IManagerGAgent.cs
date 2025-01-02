using AISmart.Agent;
using AISmart.Service;

namespace AiSmart.GAgent.TestAgent.NamingContest.ManagerAgent;

public interface IManagerGAgent:IGrainWithGuidKey
{
    Task InitAgentsAsync(InitAgentMessageGEvent initAgentMessageGEvent);
    
    Task InitGroupInfoAsync(IniNetWorkMessageGEvent iniNetWorkMessageGEvent,string groupAgentId );

    
}