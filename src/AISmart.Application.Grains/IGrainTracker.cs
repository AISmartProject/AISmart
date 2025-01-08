using System.Collections.Concurrent;
using AISmart.Agents;
using AISmart.Application.Grains.Agents.Developer;
using AISmart.Application.Grains.Agents.Group;
using AISmart.Application.Grains.Agents.MarketLeader;
using AISmart.Application.Grains.Agents.X;

namespace AISmart.Application.Grains;

public interface IGrainTracker
{
    
}

public static class GrainTracker
{
    public static ConcurrentQueue<IStateAgent<XAgentState>> XAgents = new();
    public static ConcurrentQueue<IStateAgent<MarketLeaderAgentState>> MarketLeaderAgents = new();
    public static ConcurrentQueue<IStateAgent<DeveloperAgentState>> DeveloperAgents = new();
    public static ConcurrentQueue<IStateAgent<InvestmentAgentState>> InvestmentAgents = new();
    public static ConcurrentQueue<IStateAgent<GroupAgentState>> GroupAgents = new();
}