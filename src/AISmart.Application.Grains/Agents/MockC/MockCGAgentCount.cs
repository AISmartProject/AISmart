using AISmart.Agents;

namespace AISmart.Application.Grains.Agents.MockC;

public interface IMockCGAgentCount : IGAgent
{
    Task<int> GetMockCGAgentCount();
}