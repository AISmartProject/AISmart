using AISmart.Agent;
using AISmart.Agents;
using AISmart.Service;

namespace AiSmart.GAgent.TestAgent.NamingContest.ManagerAgent;

public interface IManagerGAgent:IGAgent,IGrainWithGuidKey
{
    Task InitAgentsAsync(InitAgentMessageSEvent initAgentMessageSEvent);
    
    Task InitGroupInfoAsync(InitNetWorkMessageSEvent initNetWorkMessageSEvent,string groupAgentId );

    Task ClearAllAgentsAsync();

    Task<ManagerAgentState> GetManagerAgentStateAsync();
}