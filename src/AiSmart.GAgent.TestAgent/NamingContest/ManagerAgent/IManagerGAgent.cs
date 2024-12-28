using AISmart.Agent;
using AISmart.Service;

namespace AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;

public interface IManagerGAgent
{
    Task InitAgentsAsync(List<string> agentIdList);
    
}