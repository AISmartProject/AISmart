using AISmart.Agent;
using AISmart.Service;

namespace AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;

public interface ICreativeGAgent:IMicroAIGAgent
{
    // Task InitAgentsAsync(ContestantAgent contestantAgent);
    Task<string> GetCreativeNaming();

    Task<string> GetCreativeName();
    
    Task SetGroupIdAsync(Guid guid);

}