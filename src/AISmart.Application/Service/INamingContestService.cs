using System;
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
    public Task<AiSmartInitResponse> InitAgentsAsync(ContestAgentsDto contestAgentsDto);
    public Task<GroupResponse> InitNetworksAsync(NetworksDto networksDto);
    public Task StartGroupAsync(GroupDto groupDto);
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

    public async Task<AiSmartInitResponse> InitAgentsAsync(ContestAgentsDto contestAgentsDto)
    {
        IManagerGAgent managerGAgent = _clusterClient.GetGrain<IManagerGAgent>(GuidUtil.StringToGuid("AI-Naming-Contest"));
        
        var random = new Random();

        AgentResponse agentResponse = new AgentResponse();
        AiSmartInitResponse aiSmartInitResponse = new AiSmartInitResponse();

        if (contestAgentsDto.Network is not null )
        {
            foreach (var agent in contestAgentsDto.Network)
            {
                Guid agentId;
                AiSmartInitResponseDetail? newAgent;
                switch (agent.Label)
                {
                    case "Contestant":
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

                    case "Judge":
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
                    
                    case "Host":
                        break;

                    default:
                        break;
                }
            }
        }
        else
        {
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
        }
        
        await managerGAgent.InitAgentsAsync(new InitAgentMessageGEvent()
        {
            CreativeAgentIdList = agentResponse.ContestantAgentList.Select(agent => agent.AgentId).ToList(),
            JudgeAgentIdList = agentResponse.JudgeAgentList.Select(agent => agent.AgentId).ToList(),
            HostAgentIdList = agentResponse.HostAgentList.Select(agent => agent.AgentId).ToList(),
        });

        return aiSmartInitResponse;
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
            
            await namingContestGAgent.InitGroupInfoAsync(new IniNetWorkMessagePumpFunGEvent()
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
                
                _ = trafficAgent.AddCreativeAgent(await creativeAgent.GetCreativeName(),creativeAgent.GetPrimaryKey());

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

            await managerGAgent.InitGroupInfoAsync(new IniNetWorkMessageGEvent()
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