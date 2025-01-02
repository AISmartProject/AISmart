using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Agents.Group;
using AISmart.Common;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;
using AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;
using AiSmart.GAgent.TestAgent.NamingContest.ManagerAgent;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AiSmart.GAgent.TestAgent.NamingContest.VoteAgent;
using AISmart.Options;
using AISmart.Sender;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Orleans;
using Volo.Abp.Application.Services;

namespace AISmart.Service;

public interface INamingContestService
{
    Task<AgentResponse> InitAgentsAsync(ContestAgentsDto contestAgentsDto);
    Task ClearAllAgentsAsync();
    Task<GroupResponse> InitNetworksAsync(NetworksDto networksDto);
    Task StartGroupAsync(GroupDto groupDto);
}

public class NamingContestService : ApplicationService,INamingContestService
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
        IManagerGAgent managerGAgent = _clusterClient.GetGrain<IManagerGAgent>(GuidUtil.StringToGuid("AI-Naming-Contest"));
        
        var random = new Random();

        AgentResponse agentResponse = new AgentResponse();

        foreach (var contestant in contestAgentsDto.ContestantAgentList)
        {
            var agentId = Guid.NewGuid();
            var creativeAgent = _clusterClient.GetGrain<ICreativeGAgent>(agentId);
            await creativeAgent.SetAgent(contestant.Name, contestant.Bio);
            
            var newAgent = new AgentReponse()
            {
                AgentId = agentId.ToString(),
                Name = contestant.Name
            };

            // Add the new agent to the contestant list
            agentResponse.ContestantAgentList.Add(newAgent);
        }

        foreach (var judge in contestAgentsDto.JudgeAgentList)
        {
            var agentId = Guid.NewGuid();
            var judgeAgent = _clusterClient.GetGrain<IJudgeGAgent>(agentId);
        
            await judgeAgent.SetAgent(judge.Name, judge.Bio);

            var newAgent = new AgentReponse()
            {
                AgentId = agentId.ToString(),
                Name = judge.Name
            };

            // Add the new agent to the contestant list
            agentResponse.JudgeAgentList.Add(newAgent);
            
        }

        await managerGAgent.InitAgentsAsync(new InitAgentMessageSEvent()
        {
            CreativeAgentIdList = agentResponse.ContestantAgentList.Select(agent => agent.AgentId).ToList(),
            JudgeAgentIdList = agentResponse.JudgeAgentList.Select(agent => agent.AgentId).ToList(),
            HostAgentIdList = agentResponse.HostAgentList.Select(agent => agent.AgentId).ToList(),
        });

        return agentResponse;
    }

    public async Task ClearAllAgentsAsync()
    {
        IManagerGAgent managerGAgent = _clusterClient.GetGrain<IManagerGAgent>(GuidUtil.StringToGuid("AI-Naming-Contest"));
        await managerGAgent.ClearAllAgentsAsync();
    }

    public async Task<GroupResponse> InitNetworksAsync(NetworksDto networksDto)
    {
        
        IManagerGAgent managerGAgent = _clusterClient.GetGrain<IManagerGAgent>(GuidUtil.StringToGuid("AI-Naming-Contest"));
        
        GroupResponse groupResponse = new GroupResponse();
        foreach (var network in networksDto.Networks)
        {
            Guid groupAgentId = Guid.NewGuid();

            var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(groupAgentId);
            var trafficAgent = _clusterClient.GetGrain<IFirstTrafficGAgent>(Guid.NewGuid());
            var namingContestGAgent = _clusterClient.GetGrain<IPumpFunNamingContestGAgent>(Guid.NewGuid());


            await groupAgent.RegisterAsync(trafficAgent);
            await groupAgent.RegisterAsync(namingContestGAgent);
            
            await namingContestGAgent.InitGroupInfoAsync(new IniNetWorkMessagePumpFunSEvent()
            {
                CallBackUrl = network.CallbackAddress,
                Name = network.Name,
                Round = network.Round,
                CreativeAgentIdList = network.ConstentList,
                JudgeAgentIdList = network.JudgeList,
                ScoreAgentIdList = network.ScoreList,
                HostAgentIdList = network.HostList

            });


            foreach (var agentId in network.ConstentList)
            {
                var creativeAgent = _clusterClient.GetGrain<ICreativeGAgent>(Guid.Parse(agentId));

                MicroAIGAgentState microAigAgentState = await creativeAgent.GetAgentState();
                
                _ = trafficAgent.AddCreativeAgent(microAigAgentState.AgentName,creativeAgent.GetPrimaryKey());

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

            await managerGAgent.InitGroupInfoAsync(new InitNetWorkMessageSEvent()
            {
                CallBackUrl = network.CallbackAddress,
                Name = network.Name,
                Round = network.Round,
                CreativeAgentIdList = network.ConstentList,
                JudgeAgentIdList = network.JudgeList,
                ScoreAgentIdList = network.ScoreList,
                HostAgentIdList = network.HostList,
                GroupAgentId = groupAgentId.ToString()

            }, groupAgentId.ToString());
        }
        
        
        IVoteCharmingGAgent voteCharmingGAgent = _clusterClient.GetGrain<IVoteCharmingGAgent>(GuidUtil.StringToGuid("AI-Most-Charming-Naming-Contest"));
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
        await publishingAgent.RegisterAsync(voteCharmingGAgent);

        var round = networksDto.Networks.FirstOrDefault()!.Round;
        await publishingAgent.PublishEventAsync(new InitVoteCharmingEvent()
        {
            GrainGuidList = groupResponse.GroupDetails.Select(g=>Guid.Parse(g.GroupId)).ToList(),
            Round = Convert.ToInt32(round)
        });
        
        return groupResponse;
    }

    public async Task StartGroupAsync(GroupDto groupDto)
    {
        foreach (var groupId in groupDto.GroupIdList)
        {
            var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.Parse(groupId));
            var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
            // await publishingAgent.ActivateAsync();
            await publishingAgent.RegisterAsync(groupAgent);
            await publishingAgent.PublishEventAsync(new GroupStartEvent());
        }
    }
}