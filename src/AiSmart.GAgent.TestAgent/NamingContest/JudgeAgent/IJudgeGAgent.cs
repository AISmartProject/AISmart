using AISmart.Agent;

namespace AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;

public interface IJudgeGAgent:IMicroAIGAgent
{
    Task SetGroupIdAsync(Guid guid);
}