using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Agents.Group;
using AISmart.Agents.Messaging;
using AISmart.Application.Grains.Agents.Messaging;
using AISmart.Common;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;
using AiSmart.GAgent.TestAgent.NamingContest.HostAgent;
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
using Orleans.Runtime;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace AISmart.Service;

public interface INamingContestService : ISingletonDependency
{
    Task<AiSmartInitResponse> InitAgentsAsync(ContestAgentsDto contestAgentsDto);
    Task ClearAllAgentsAsync();
    Task<GroupResponse> InitNetworksAsync(NetworksDto networksDto);
    Task<List<string>> LoadTest();
    Task<int> VerifyLoadTest(List<string> ids);
    Task<GroupStartResponse> StartGroupAsync(GroupStartDto groupStartDto);
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

    public async Task<AiSmartInitResponse> InitAgentsAsync(ContestAgentsDto contestAgentsDto)
    {
        IManagerGAgent managerGAgent =
            _clusterClient.GetGrain<IManagerGAgent>(GuidUtil.StringToGuid("AI-Naming-Contest"));

        var random = new Random();

        AiSmartInitResponse aiSmartInitResponse = new AiSmartInitResponse();


        if (contestAgentsDto.Network is not null)
        {
            foreach (var agent in contestAgentsDto.Network)
            {
                Guid agentId;
                AiSmartInitResponseDetail? newAgent;
                switch (agent.Label)
                {
                    case NamingContestConstant.AgentLabelContestant:
                        agentId = Guid.NewGuid();
                        var creativeAgent = _clusterClient.GetGrain<ICreativeGAgent>(agentId);
                        await creativeAgent.SetAgent(agent.Name, agent.Bio);

                        newAgent = new AiSmartInitResponseDetail()
                        {
                            AgentId = agentId.ToString(),
                            AgentName = agent.Name,
                            Label = agent.Label
                        };
                        // Add the new agent to the contestant list
                        aiSmartInitResponse.Details.Add(newAgent);
                        break;

                    case NamingContestConstant.AgentLabelJudge:
                        agentId = Guid.NewGuid();
                        var judgeAgent = _clusterClient.GetGrain<IJudgeGAgent>(agentId);

                        await judgeAgent.SetAgent(agent.Name, agent.Bio);

                        newAgent = new AiSmartInitResponseDetail()
                        {
                            AgentId = agentId.ToString(),
                            AgentName = agent.Name,
                            Label = agent.Label
                        };

                        // Add the new agent to the contestant list
                        aiSmartInitResponse.Details.Add(newAgent);
                        break;

                    case NamingContestConstant.AgentLabelHost:
                        agentId = Guid.NewGuid();
                        var hostAgent = _clusterClient.GetGrain<IHostGAgent>(agentId);

                        await hostAgent.SetAgent(agent.Name, agent.Bio);

                        newAgent = new AiSmartInitResponseDetail()
                        {
                            AgentId = agentId.ToString(),
                            AgentName = agent.Name,
                            Label = agent.Label
                        };

                        // Add the new agent to the contestant list
                        aiSmartInitResponse.Details.Add(newAgent);
                        break;

                    default:
                        break;
                }
            }
        }

        var creativeAgentIdList = aiSmartInitResponse.Details
            .FindAll(agent => agent.Label == NamingContestConstant.AgentLabelContestant).Select(agent => agent.AgentId)
            .ToList();
        var judgeAgentIdList = aiSmartInitResponse.Details
            .FindAll(agent => agent.Label == NamingContestConstant.AgentLabelJudge).Select(agent => agent.AgentId)
            .ToList();
        var hostAgentIdList = aiSmartInitResponse.Details
            .FindAll(agent => agent.Label == NamingContestConstant.AgentLabelHost).Select(agent => agent.AgentId)
            .ToList();

        await managerGAgent.InitAgentsAsync(new InitAgentMessageSEvent()
        {
            CreativeAgentIdList = creativeAgentIdList,
            JudgeAgentIdList = judgeAgentIdList,
            HostAgentIdList = hostAgentIdList
        });

        return aiSmartInitResponse;
    }

    public async Task ClearAllAgentsAsync()
    {
        IManagerGAgent managerGAgent =
            _clusterClient.GetGrain<IManagerGAgent>(GuidUtil.StringToGuid("AI-Naming-Contest"));
        await managerGAgent.ClearAllAgentsAsync();
    }

    public async Task<GroupResponse> InitNetworksAsync(NetworksDto networksDto)
    {
        IManagerGAgent managerGAgent =
            _clusterClient.GetGrain<IManagerGAgent>(GuidUtil.StringToGuid("AI-Naming-Contest"));

        GroupResponse groupResponse = new GroupResponse();
        Dictionary<string, bool> judgeDic = networksDto.Networks.SelectMany(network => network.JudgeList)
            .ToDictionary(judge => judge, judge => false);
        
        var voteCharmingGAgent =
            _clusterClient.GetGrain<IVoteCharmingGAgent>(Helper.GetVoteCharmingGrainId(networksDto.Round,
                networksDto.Step));
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
        await publishingAgent.RegisterAsync(voteCharmingGAgent);
        
        var groupList = new List<Guid>();
        foreach (var network in networksDto.Networks)
        {
            Guid groupAgentId = Guid.NewGuid();

            var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(groupAgentId);


            ITrafficGAgent trafficAgent;

            if (network.Round == "1")
            {
                trafficAgent = _clusterClient.GetGrain<IFirstTrafficGAgent>(Guid.NewGuid());
            }
            else
            {
                trafficAgent = _clusterClient.GetGrain<ISecondTrafficGAgent>(Guid.NewGuid());
                _ = ((ISecondTrafficGAgent)trafficAgent).SetAskJudgeNumber(20);
                _ = ((ISecondTrafficGAgent)trafficAgent).SetRoundNumber(Convert.ToInt32(network.Round));
            }

            await trafficAgent.SetStepCount(network.Step);
            groupList.Add(groupAgent.GetPrimaryKey());
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
                HostAgentIdList = network.HostList,
                GroupId = groupAgent.GetPrimaryKey(),
                MostCharmingBackUrl = _nameContestOptions.MostCharmingCallback,
                MostCharmingGroupId = voteCharmingGAgent.GetPrimaryKey(),
            });

            foreach (var agentId in network.ConstentList)
            {
                var creativeAgent = _clusterClient.GetGrain<ICreativeGAgent>(Guid.Parse(agentId));

                _ = trafficAgent.AddCreativeAgent(await creativeAgent.GetCreativeName(), creativeAgent.GetPrimaryKey());

                await groupAgent.RegisterAsync(creativeAgent);
            }

            foreach (var agentId in network.JudgeList)
            {
                var judgeAgent = _clusterClient.GetGrain<IJudgeGAgent>(Guid.Parse(agentId));
                if (judgeDic[agentId])
                {
                    judgeAgent = await judgeAgent.Clone();
                }
                else
                {
                    judgeDic[agentId] = true;
                }

                _ = trafficAgent.AddJudgeAgent(judgeAgent.GetPrimaryKey());

                await groupAgent.RegisterAsync(judgeAgent);
            }

            var scoreListExcludingJudgeList = network.ScoreList.Except(network.JudgeList);


            foreach (var agentId in scoreListExcludingJudgeList)
            {
                var judgeAgent = _clusterClient.GetGrain<IJudgeGAgent>(Guid.Parse(agentId));

                // await trafficAgent.AddJudgeAgent(judgeAgent.GetPrimaryKey());

                await groupAgent.RegisterAsync(judgeAgent);
            }

            foreach (var agentId in network.HostList)
            {
                var hostGAgent = _clusterClient.GetGrain<IHostGAgent>(Guid.Parse(agentId));

                await trafficAgent.AddHostAgent(hostGAgent.GetPrimaryKey());

                await groupAgent.RegisterAsync(hostGAgent);
            }

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

        // init vote mostcharming log
        var round = networksDto.Networks.FirstOrDefault()!.Round;
        if (!NamingConstants.RoundTotalBatchesMap.TryGetValue(round, out var totalBatches))
        {
            totalBatches = NamingConstants.DefaultTotalTotalBatches;
        }

        await publishingAgent.PublishEventAsync(new InitVoteCharmingEvent()
        {
            CreativeGuidList = new List<Guid>(),
            JudgeGuidList = new List<Guid>(),
            Round = Convert.ToInt32(round),
            TotalBatches = totalBatches,
            groupList = groupList,
            TotalGroupCount = groupList.Count,
        });

        return groupResponse;
    }

    public async Task<List<string>> LoadTest()
    {
        var parentId = Guid.NewGuid();
        var parentAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(parentId);

        List<string> ids = [];
        var maxAgents = 400;
        for (var i = 0; i < maxAgents; ++i)
        {
            var messagingAgentId = Guid.NewGuid();
            var messagingAgent = _clusterClient.GetGrain<IMessagingGAgent>(messagingAgentId);
            await parentAgent.RegisterAsync(messagingAgent);

            ids.Add(messagingAgent.GetGrainId().ToString());
        }

        _logger.LogInformation("Added {maxAgents} agents to parent agent {ParentAgentId}.", maxAgents,
            parentAgent.GetGrainId().ToString());

        var publisher = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
        await parentAgent.RegisterAsync(publisher);
        await publisher.PublishEventAsync(new SendEvent()
        {
            Message = "Hello, World!"
        });

        _logger.LogInformation("Published event to parent agent {ParentAgentId}.", parentAgent.GetGrainId().ToString());

        return ids;
    }

    public async Task<int> VerifyLoadTest(List<string> ids)
    {
        var completed = 0;
        foreach (var id in ids)
        {
            var grainId = GrainId.Parse(id);
            var messagingGAgent = _clusterClient.GetGrain<IMessagingGAgent>(grainId);
            var received = await messagingGAgent.GetReceivedMessagesAsync();
            completed += (received == 400) ? 1 : 0;
        }

        return completed;
    }

    public async Task<GroupStartResponse> StartGroupAsync(GroupStartDto groupStartDto)
    {
        GroupStartResponse groupStartResponse = new GroupStartResponse();

        foreach (var groupId in groupStartDto.GroupIdList)
        {
            await StartOneGroupAsync(groupId, groupStartResponse);
        }

        return groupStartResponse;
    }

    private async Task StartOneGroupAsync(string groupId, GroupStartResponse groupStartResponse)
    {
        try
        {
            var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.Parse(groupId));
            var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
            // await publishingAgent.ActivateAsync();
            await publishingAgent.RegisterAsync(groupAgent);
            await publishingAgent.PublishEventAsync(new GroupStartEvent()
            {
                MessageId = Guid.NewGuid().ToString(),
                Message = "Name a portable smart translator targeting the youth market."
            });
            groupStartResponse.SuccessGroupIdList.Add(groupId);
        }
        catch (Exception e)
        {
            groupStartResponse.FailGroupIdList.Add(groupId);
            _logger.LogError(e, "groupId:{groupId} StartOneGroupAsync error,", groupId);
        }
    }
}