using AISmart.Agent;
using AISmart.Service;

namespace AiSmart.GAgent.TestAgent.NamingContest.ManagerAgent;

public interface IManagerGAgent:IGrainWithGuidKey
{
    Task InitAgentsAsync(InitAgentMessageSEvent initAgentMessageSEvent);
    
    Task InitGroupInfoAsync(IniNetWorkMessageSEvent iniNetWorkMessageSEvent,string groupAgentId );

    
}