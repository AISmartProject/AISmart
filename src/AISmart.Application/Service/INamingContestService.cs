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
            var agentId = Guid.NewGuid();
            var creativeAgent = _clusterClient.GetGrain<ICreativeGAgent>(agentId);
            await creativeAgent.InitAgentsAsync(contestant);
            
            var newAgent = new AgentReponse()
            {
                AgentId = agentId.ToString(),
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
            var agentId = Guid.NewGuid();
            var judgeAgent = _clusterClient.GetGrain<IJudgeGAgent>(agentId);

            var newAgent = new AgentReponse()
            {
                AgentId = agentId.ToString(),
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
        foreach (var network in networksDto.Networks)
        {
            Guid groupAgentId = Guid.NewGuid();

            var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(groupAgentId);
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

                await trafficAgent.AddJudgeAgent(judgeAgent.GetPrimaryKey());

                await groupAgent.RegisterAsync(judgeAgent);
            }

            foreach (var agentId in network.HostList)
            {
                Console.WriteLine($"Host agentId: {agentId}");
            }

            Console.WriteLine($"Callback Address: {network.CallbackAddress}");
            Console.WriteLine($"Network Name: {network.Name}");
            
            var groupDetail = new GroupDetail()
            {
                GroupId = groupAgentId.ToString(),
                Name = network.Name
            };

            // Add the new agent to the  list
            groupResponse.GroupDetails.Add(groupDetail);
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