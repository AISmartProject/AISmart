using AISmart.Agent;
using AiSmart.GAgent.TestAgent.NamingContest.VoteAgent;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

public interface ITrafficGAgent:IMicroAIGAgent
{
    Task AddCreativeAgent(string creativeName, Guid creativeGrainId);
    Task AddJudgeAgent(Guid judgeGrainId);
    
    Task AddHostAgent(Guid judgeGrainId);

    Task SetStepCount(int step);

    Task<int> GetProcessStep();
}