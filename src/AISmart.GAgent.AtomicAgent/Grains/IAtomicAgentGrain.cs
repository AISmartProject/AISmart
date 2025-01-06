using Orleans;
using AISmart.GAgent.AtomicAgent.Models;

namespace AISmart.GAgent.AtomicAgent.Grains;

public interface IAtomicAgentGrain : IGrainWithStringKey
{
    Task<AgentData> GetAgentAsync();
    Task CreateAgentAsync(AgentData agent);
    Task UpdateAgentAsync(AgentData agent);
    Task DeleteAgentAsync();
}