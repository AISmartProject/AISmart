using System;
using System.Threading.Tasks;
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
    public Task<AgentResponse> InitAgentsAsync(ContestAgentsDto contestAgentsDto, StringValues token);
    public Task<GroupResponse> InitNetworksAsync(NetworksDto networksDto, StringValues token);
    public Task StartGroupAsync(GroupDto groupDto, StringValues token);
    
}

public class NamingContestService : ApplicationService, INamingContestService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<NamingContestService> _logger;
    private readonly TelegramTestOptions _telegramTestOptions;
    private readonly TelegramOptions _telegramOptions;

    public NamingContestService(IOptions<TelegramTestOptions> telegramTestOptions, IOptions<TelegramOptions> telegramOption,
        IClusterClient clusterClient,
        ILogger<NamingContestService> logger)
    {
        _clusterClient = clusterClient;
        _telegramTestOptions = telegramTestOptions.Value;
        _logger = logger;
        _telegramOptions = telegramOption.Value;
    }

    public async Task<AgentResponse> InitAgentsAsync(ContestAgentsDto contestAgentsDto, StringValues token)
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
            
            
            foreach (var goal in contestant.Goals)
            {
                var temperature = random.NextDouble();

                await creativeAgent.SetAgentWithTemperatureAsync(
                    goal.Action,
                    $"{goal.Description} You must provide a definite name for the user's input regarding a naming question, without including any additional information.",
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
            agentResponse.ContestantAgentList.Add(newAgent);
            
            foreach (var goal in judge.Goals)
            {
                var temperature = random.NextDouble();

                await judgeAgent.SetAgentWithTemperatureAsync(
                    goal.Action,
                    $"{goal.Description} You must provide a definite name for the user's input regarding a naming question, without including any additional information.",
                    (float)temperature);
            }
        }

        return agentResponse;
    }

    public async Task<GroupResponse> InitNetworksAsync(NetworksDto networksDto, StringValues token)
    {
        GroupResponse groupResponse = new GroupResponse();
        
        for (int i = 0; i < networksDto.Networks.Count; i++)
        {
            var network = networksDto.Networks[i];
            
            var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());

            var trafficAgent = _clusterClient.GetGrain<ITrafficGAgent>(Guid.NewGuid());
            await trafficAgent.SetAgent("Traffic",
                "You need to determine whether the user's input is a question about naming. If it is, please return 'True'; otherwise, return 'False'.");
            
            await groupAgent.RegisterAsync(trafficAgent);
            
            
            foreach (var agentId in network.ConstentList)
            {
                
                var creativeAgent = _clusterClient.GetGrain<ICreativeGAgent>(Guid.Parse(agentId));
                
                await trafficAgent.AddCreativeAgent(creativeAgent.GetPrimaryKey());
                
                await groupAgent.RegisterAsync(creativeAgent);

            }
            
            foreach (var agentId in network.JudgeList)
            {
              
                var judgeAgent = _clusterClient.GetGrain<IJudgeGAgent>(Guid.Parse(agentId));
                
                await trafficAgent.AddCreativeAgent(judgeAgent.GetPrimaryKey());
                
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

    public async Task StartGroupAsync(GroupDto groupDto, StringValues token)
    {
        foreach (var groupId in groupDto.GroupIdList)
        {
            var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.Parse(groupId));
            var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
            // await publishingAgent.ActivateAsync();
            await publishingAgent.PublishToAsync(groupAgent);
            publishingAgent.PublishToAsync()
        }
        throw new System.NotImplementedException();
    }
}    