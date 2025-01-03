using AISmart.Agents;

namespace AISmart.Application.Grains.Agents.LoadTestAgent;

public interface ILoadTestGAgentCount : IGAgent
{
    Task<(int Number, DateTime LastEventTimestamp)> GetLoadTestGAgentCount();
}