using AISmart.Agent;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

public interface ITrafficGAgent:IMicroAIGAgent
{
    Task AddCreativeAgent(Guid creativeGrainId);
}