using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AISmart.Agent;
using AISmart.Agents;
using AISmart.Agents.Group;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;
using AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.Options;
using AISmart.Sender;
using AISmart.Telegram;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Orleans;
using Volo.Abp.Application.Services;

namespace AISmart.Service;

public interface INamingContestService
{
    public Task<AgentResponse> InitAgentsAsync(ContestAgentsDto contestAgentsDto);
    public Task<GroupResponse> InitNetworksAsync(NetworksDto networksDto);
    public Task StartGroupAsync(GroupDto groupDto);
}

public class NamingContestService : INamingContestService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<NamingContestService> _logger;
    private readonly NameContestOptions _nameContestOptions;

    public NamingContestService(
        IClusterClient clusterClient,
        ILogger<NamingContestService> logger,
        IOptions<NameContestOptions> nameContestOptions)
    {
        _clusterClient = clusterClient;
        _logger = logger;
        _nameContestOptions = nameContestOptions.Value;
    }

    public async Task<AgentResponse> InitAgentsAsync(ContestAgentsDto contestAgentsDto)
    {
        var random = new Random();

        AgentResponse agentResponse = new AgentResponse();

        foreach (var contestant in contestAgentsDto.ContestantAgentList)
        {
            var creativeAgent = _clusterClient.GetGrain<ICreativeGAgent>(Guid.NewGuid());
            var newAgent = new AgentReponse()
            {
                AgentId = creativeAgent.GetGrainId().ToString(),
                Name = contestant.Name
            };

            // Add the new agent to the contestant list
            agentResponse.ContestantAgentList.Add(newAgent);

            foreach (var item in _nameContestOptions.CreativeGAgent)
            {
                var temperature = random.NextDouble();

                await creativeAgent.SetAgentWithTemperatureAsync(
                    item.Key,
                    $"{item.Value}",
                    (float)temperature);
            }
        }

        foreach (var judge in contestAgentsDto.JudgeAgentList)
        {
            var judgeAgent = _clusterClient.GetGrain<IJudgeGAgent>(Guid.NewGuid());

            var newAgent = new AgentReponse()
            {
                AgentId = judgeAgent.GetGrainId().ToString(),
                Name = judge.Name
            };

            // Add the new agent to the contestant list
            agentResponse.JudgeAgentList.Add(newAgent);

            foreach (var item in _nameContestOptions.JudgeGAgent)
            {
                var temperature = random.NextDouble();

                await judgeAgent.SetAgentWithTemperatureAsync(
                    item.Key,
                    $"{item.Value}",
                    (float)temperature);
            }
        }

        return agentResponse;
    }

    public async Task<GroupResponse> InitNetworksAsync(NetworksDto networksDto)
    {
        GroupResponse groupResponse = new GroupResponse();

        for (int i = 0; i < networksDto.Networks.Count; i++)
        {
            var network = networksDto.Networks[i];

            var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
            var trafficAgent = _clusterClient.GetGrain<IFirstTrafficGAgent>(Guid.NewGuid());
            var namingContestGAgent = _clusterClient.GetGrain<INamingContestGAgent>(Guid.NewGuid());


            await groupAgent.RegisterAsync(trafficAgent);
            await groupAgent.RegisterAsync(namingContestGAgent);

            _ = namingContestGAgent.SetCallBackURL(network.CallbackAddress);


            foreach (var agentId in network.ConstentList)
            {
                var creativeAgent = _clusterClient.GetGrain<ICreativeGAgent>(Guid.Parse(agentId));

                _ = trafficAgent.AddCreativeAgent(creativeAgent.GetPrimaryKey());

                await groupAgent.RegisterAsync(creativeAgent);
            }

            foreach (var agentId in network.JudgeList)
            {
                var judgeAgent = _clusterClient.GetGrain<IJudgeGAgent>(Guid.Parse(agentId));

                _ = trafficAgent.AddJudgeAgent(judgeAgent.GetPrimaryKey());

                await groupAgent.RegisterAsync(judgeAgent);
            }

            foreach (var agentId in network.ScoreList)
            {
                var judgeAgent = _clusterClient.GetGrain<IJudgeGAgent>(Guid.Parse(agentId));

                await trafficAgent.AddCreativeAgent(judgeAgent.GetPrimaryKey());

                await groupAgent.RegisterAsync(judgeAgent);
            }

            foreach (var agentId in network.HostList)
            {
                Console.WriteLine($"Host agentId: {agentId}");
            }

            Console.WriteLine($"Callback Address: {network.CallbackAddress}");
            Console.WriteLine($"Network Name: {network.Name}");
            groupResponse.GroupDetails[i].GroupId = groupAgent.GetGrainId().ToString();
            groupResponse.GroupDetails[i].Name = network.Name;
        }

        return groupResponse;
    }

    public async Task StartGroupAsync(GroupDto groupDto)
    {
        foreach (var groupId in groupDto.GroupIdList)
        {
            var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.Parse(groupId));
            var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
            // await publishingAgent.ActivateAsync();
            await publishingAgent.PublishToAsync(groupAgent);
            await publishingAgent.PublishEventAsync(new GroupStartEvent());
        }
    }
}