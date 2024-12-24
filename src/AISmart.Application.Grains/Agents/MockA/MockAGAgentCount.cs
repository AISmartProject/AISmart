using AISmart.Agents;

namespace AISmart.Application.Grains.Agents.MockA;

public interface IMockAGAgentCount : IGAgent
{
    Task<int> GetMockAGAgentCount();
}