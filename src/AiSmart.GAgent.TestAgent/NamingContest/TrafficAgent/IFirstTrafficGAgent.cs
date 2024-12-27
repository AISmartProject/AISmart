using AISmart.Agent;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

public interface IFirstTrafficGAgent:IMicroAIGAgent
{
    Task AddCreativeAgent(Guid creativeGrainId);
}