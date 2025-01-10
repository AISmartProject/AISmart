using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.Agents.Group;
using AISmart.Application.Grains.Agents.Group;
using AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;
using AISmart.Sender;
using Orleans;
using Volo.Abp.Application.Services;

namespace AISmart;

public interface IReloadTestAppService
{
    Task<Dictionary<string, Guid>> SetGroupAsync();
    Task<bool> PublishEventToGroupAsync(string groupGAgentGuid);
    Task<string> CheckGroupMemberState(string groupMemberGuid);
}

public class ReloadTestAppService : ApplicationService, IReloadTestAppService
{
    private readonly IClusterClient _clusterClient;

    public ReloadTestAppService(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    public async Task<Dictionary<string, Guid>> SetGroupAsync()
    {
        var groupMember1 = _clusterClient.GetGrain<IStateGAgent<GroupTestGAgentState>>(Guid.NewGuid());
        var groupMember2 = _clusterClient.GetGrain<IStateGAgent<GroupTestGAgentState>>(Guid.NewGuid());
        var groupMember3 = _clusterClient.GetGrain<IStateGAgent<GroupTestGAgentState>>(Guid.NewGuid());
        var groupMember4 = _clusterClient.GetGrain<IStateGAgent<GroupTestGAgentState>>(Guid.NewGuid());
        var groupMember5 = _clusterClient.GetGrain<IStateGAgent<GroupTestGAgentState>>(Guid.NewGuid());

        var groupGAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
        await groupGAgent.RegisterAsync(groupMember1);
        await groupGAgent.RegisterAsync(groupMember2);
        await groupGAgent.RegisterAsync(groupMember3);
        await groupGAgent.RegisterAsync(groupMember4);
        await groupGAgent.RegisterAsync(groupMember5);

        return new Dictionary<string, Guid>
        {
            ["GroupGAgent"] = groupGAgent.GetPrimaryKey(),
            ["MemberGAgent1"] = groupMember1.GetPrimaryKey(),
            ["MemberGAgent2"] = groupMember2.GetPrimaryKey(),
            ["MemberGAgent3"] = groupMember3.GetPrimaryKey(),
            ["MemberGAgent4"] = groupMember4.GetPrimaryKey(),
            ["MemberGAgent5"] = groupMember5.GetPrimaryKey(),
        };
    }

    public async Task<bool> PublishEventToGroupAsync(string groupGAgentGuid)
    {
        var groupGuid = Guid.Parse(groupGAgentGuid);
        var publishingGAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
        var groupGAgent =
            _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(groupGuid);
        await groupGAgent.ActivateAsync();
        await publishingGAgent.RegisterAsync(groupGAgent);
        await publishingGAgent.PublishEventAsync(new GroupReloadTestEvent
        {
            GroupManagerGuid = groupGuid
        });
        return true;
    }

    public async Task<string> CheckGroupMemberState(string groupMemberGuid)
    {
        var memberGuid = Guid.Parse(groupMemberGuid);
        var groupMember = _clusterClient.GetGrain<IStateGAgent<GroupTestGAgentState>>(memberGuid);
        var state = await groupMember.GetStateAsync();
        return $"{state.GroupManagerGuid}, {state.CalledCount.ToString()}";
    }

    public async Task<string> SetAComplicatedGroupAsync()
    {
        var groupMember1 = _clusterClient.GetGrain<IStateGAgent<GroupTestGAgentState>>(Guid.NewGuid());
        var groupMember2 = _clusterClient.GetGrain<IStateGAgent<GroupTestGAgentState>>(Guid.NewGuid());
        var groupMember3 = _clusterClient.GetGrain<IStateGAgent<GroupTestGAgentState>>(Guid.NewGuid());

        var groupMember1A = _clusterClient.GetGrain<IStateGAgent<GroupTestGAgentState>>(Guid.NewGuid());
        var groupMember1B = _clusterClient.GetGrain<IStateGAgent<GroupTestGAgentState>>(Guid.NewGuid());
        await groupMember1.RegisterAsync(groupMember1A);
        await groupMember1.RegisterAsync(groupMember1B);

        var groupMember1Aa = _clusterClient.GetGrain<IStateGAgent<GroupTestGAgentState>>(Guid.NewGuid());
        var groupMember1Ab = _clusterClient.GetGrain<IStateGAgent<GroupTestGAgentState>>(Guid.NewGuid());
        await groupMember1A.RegisterAsync(groupMember1Aa);
        await groupMember1A.RegisterAsync(groupMember1Ab);

        var groupGAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
        await groupGAgent.RegisterAsync(groupMember1);
        await groupGAgent.RegisterAsync(groupMember2);
        await groupGAgent.RegisterAsync(groupMember3);

        return groupGAgent.GetPrimaryKey().ToString();
    }

    public async Task<Dictionary<string, int>> GetCreativeExecuteStatus(List<string> creativeList)
    {
        var result = new Dictionary<string, int>();
        foreach (var item in creativeList)
        {
            var creativeId = Guid.Parse(item);

            var creativeAgent = _clusterClient.GetGrain<ICreativeGAgent>(creativeId);
            var step = await creativeAgent.GetExecuteStep();
            result.Add(item, step);
        }

        return result;
    }
}