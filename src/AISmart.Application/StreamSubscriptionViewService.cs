using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.Agents.Group;
using Orleans;
using Volo.Abp.Application.Services;

namespace AISmart;

public interface IStreamSubscriptionViewService
{
    Task<List<string>> ViewGroupSubscribersAsync(string guid);
}

public class StreamSubscriptionViewService : ApplicationService, IStreamSubscriptionViewService
{
    private readonly IClusterClient _clusterClient;

    public StreamSubscriptionViewService(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    public async Task<List<string>> ViewGroupSubscribersAsync(string guid)
    {
        var gAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.Parse(guid));
        var subscribers = await gAgent.GetSubscribersAsync();
        return subscribers.Select(s => s.ToString()).ToList();
    }
}