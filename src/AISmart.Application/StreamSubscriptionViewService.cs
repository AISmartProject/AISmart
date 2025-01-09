using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.Agents.Group;
using AISmart.Application.Grains.Agents.Group;
using Orleans;
using Orleans.Metadata;
using Orleans.Runtime;
using Volo.Abp.Application.Services;

namespace AISmart;

public interface IStreamSubscriptionViewService
{
    Task<List<string>> ViewGroupSubscribersAsync(string guid);
}

public class StreamSubscriptionViewService : ApplicationService, IStreamSubscriptionViewService
{
    private readonly IClusterClient _clusterClient;
    private readonly GrainTypeResolver _grainTypeResolver;

    public StreamSubscriptionViewService(IClusterClient clusterClient, GrainTypeResolver grainTypeResolver)
    {
        _clusterClient = clusterClient;
        _grainTypeResolver = grainTypeResolver;
    }

    public async Task<List<string>> ViewGroupSubscribersAsync(string guid)
    {
        var gAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.Parse(guid));
        var subscribers = await gAgent.GetChildrenAsync();
        return subscribers.Select(s => s.ToString()).ToList();
    }

    public async Task<Dictionary<string, List<string>>> ViewGroupTreeAsync(string guid)
    {
        var result = new Dictionary<string, List<string>>();
        await BuildGroupTreeAsync(GrainId.Create(_grainTypeResolver.GetGrainType(typeof(GroupGAgent)), Guid.Parse(guid).ToString("N")), result);
        return result;
    }

    private async Task BuildGroupTreeAsync(GrainId grainId, Dictionary<string, List<string>> result)
    {
        var gAgent = _clusterClient.GetGrain<IGAgent>(grainId);
        var subscribers = await gAgent.GetChildrenAsync();
        if (subscribers.IsNullOrEmpty())
        {
            return;
        }

        var subscriberIds = subscribers.Select(s => s.ToString()).ToList();
        result[grainId.ToString()] = subscriberIds;

        foreach (var subscriberId in subscribers)
        {
            await BuildGroupTreeAsync(subscriberId, result);
        }
    }
}