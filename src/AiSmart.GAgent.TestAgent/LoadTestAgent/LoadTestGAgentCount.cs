using AISmart.Agents;

namespace AiSmart.GAgent.TestAgent.LoadTestAgent;

public interface ILoadTestGAgentCount : IGAgent
{
    Task<(int Number, DateTime StartTimestamp, DateTime EndTimestamp)> GetLoadTestGAgentInfo();
}