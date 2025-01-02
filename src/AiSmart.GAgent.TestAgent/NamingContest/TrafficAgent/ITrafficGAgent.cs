using AISmart.Agent;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

public interface ITrafficGAgent:IMicroAIGAgent
{
    Task AddCreativeAgent(string creativeName, Guid creativeGrainId);
    Task AddJudgeAgent(Guid judgeGrainId);
}