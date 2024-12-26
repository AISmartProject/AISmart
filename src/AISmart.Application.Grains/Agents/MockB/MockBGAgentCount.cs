using AISmart.Agents;

namespace AISmart.Application.Grains.Agents.MockB;

public interface IMockBGAgentCount : IGAgent
{
    Task<int> GetMockBGAgentCount();
}