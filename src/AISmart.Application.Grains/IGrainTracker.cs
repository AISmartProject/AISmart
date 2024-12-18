using System.Collections.Concurrent;
using AISmart.Agents;
using AISmart.Agents.Developer;
using AISmart.Agents.Group;
using AISmart.Agents.Investment;
using AISmart.Agents.MarketLeader;
using AISmart.Agents.X;
using AISmart.Application.Grains.Agents.Developer;
using AISmart.Application.Grains.Agents.Group;
using AISmart.Application.Grains.Agents.MarketLeader;
using AISmart.Application.Grains.Agents.X;
using AISmart.Sender;

namespace AISmart.Application.Grains;

public interface IGrainTracker
{
    
}

public static class GrainTracker
{
    public static ConcurrentQueue<IStateGAgent<XAgentState>> XAgents = new();
    public static ConcurrentQueue<IStateGAgent<MarketLeaderAgentState>> MarketLeaderAgents = new();
    public static ConcurrentQueue<IStateGAgent<DeveloperAgentState>> DeveloperAgents = new();
    public static ConcurrentQueue<IStateGAgent<InvestmentAgentState>> InvestmentAgents = new();
    public static ConcurrentQueue<IStateGAgent<GroupAgentState>> GroupAgents = new();
    public static ConcurrentQueue<IPublishingGAgent> PublishingGAgents = new();
}