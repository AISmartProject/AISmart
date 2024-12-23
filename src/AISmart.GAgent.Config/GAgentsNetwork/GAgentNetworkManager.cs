using System.Collections.ObjectModel;
using System.Diagnostics;
using AISmart.Agents;
using AISmart.AgentsNetwork;
using AISmart.Application.Grains.Agents.Group;
using AISmart.Options;
using Volo.Abp.DependencyInjection;

namespace AISmart.GAgent.Config.GAgentsNetwork;


public interface IAgentNetworkManager:ISingletonDependency
{
    Task InitializeAgentsAsync(AgentNetworkConfigOptions config);

    IDictionary<string, IGAgent?> GetAgentInstances();

    IEnumerable<IGAgent?> GetGroupAgents();
}

public class AgentNetworkManager:IAgentNetworkManager
{
    private readonly IGrainFactory _grainFactory;
    private readonly Dictionary<string, IGAgent?> _agentInstances = new Dictionary<string, IGAgent?>();
    private readonly List<IGAgent?> _groupAgents = new List<IGAgent?>(); // List to store group  agents


    public AgentNetworkManager(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }
    
    public IDictionary<string, IGAgent?> GetAgentInstances()
    {
        return new ReadOnlyDictionary<string, IGAgent?>(_agentInstances);
    } 
    
    public IEnumerable<IGAgent?> GetGroupAgents()
    {
        return new ReadOnlyCollection<IGAgent?>(_groupAgents);
    }

    public async Task InitializeAgentsAsync(AgentNetworkConfigOptions config)
    {
        InitAgents(config);

        await InitGroupAsync(config);
    }
    


    private async Task InitGroupAsync(AgentNetworkConfigOptions config)
    {
        if (config is { Groups: null } || config.Groups.Count == 0)
        {
            return;
        }
        foreach (var group in config.Groups)
        {
            // Register Agents in their respective groups
            var groupGAgentName = group.AgentsList.FirstOrDefault(); // Assume the first agent is the group leader
            Debug.Assert(groupGAgentName != null, "groupGAgentName should not be null.");
            Debug.Assert(_agentInstances.ContainsKey(groupGAgentName), "Group leader agent not found.");

            
            var groupGAgent =  _grainFactory.GetGrain<IGAgent>(Guid.NewGuid(),typeof(GroupGAgent).Namespace);
            
            foreach (var name in group.AgentsList)
            {
                if (_agentInstances.TryGetValue(name, out var agent))
                {
                    Debug.Assert(agent != null , "agent should not be null.");
                    await groupGAgent?.RegisterAsync(agent)!;
                }
            }

            _groupAgents.Add(groupGAgent);
        }
    }

    private void InitAgents(AgentNetworkConfigOptions config)
    {
        // Create Agents
        foreach (var contract in config.ContractsList)
        {
            switch (contract.Type)
            {
                case GAgentConsent.User:
                    var instance = _grainFactory.GetGrain<IGAgent>(Guid.NewGuid(), RemoveLastDotSegment(contract.GrainType));
                    _agentInstances[contract.Name] = instance as IGAgent;
                    break;

                default:
                    // For all other types, continue without doing anything
                    continue;
            }

        }
    }

    private static string RemoveLastDotSegment(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var lastDotIndex = input.LastIndexOf('.');
        return lastDotIndex >= 0 ? input[..lastDotIndex] : input;
    }
}