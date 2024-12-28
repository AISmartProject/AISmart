using AISmart.Agent;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

public interface IFirstTrafficGAgent:IMicroAIGAgent
{
    Task AddCreativeAgent(string creativeName, Guid creativeGrainId);
    Task AddJudgeAgent(Guid judgeGrainId);
}