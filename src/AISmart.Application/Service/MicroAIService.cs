using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Agent;
using AISmart.Agents;
using AISmart.Agents.AutoGen;
using AISmart.Agents.Group;
using AISmart.Common;
using AISmart.Events;
using AISmart.GAgent.Autogen;
using AiSmart.GAgent.TestAgent.ConclusionAgent;
using AiSmart.GAgent.TestAgent.Voter;
using AISmart.Sender;
using Microsoft.Extensions.Logging;
using Orleans;
using Volo.Abp.Application.Services;

namespace AISmart.Service;

public class MicroAIService : ApplicationService, IMicroAIService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<MicroAIService> _logger;
    private static readonly Guid PublishId = Guid.NewGuid();
    private IMicroAIService _microAiServiceImplementation;

    public MicroAIService(IClusterClient clusterClient, ILogger<MicroAIService> logger)
    {
        _clusterClient = clusterClient;
        _logger = logger;
    }

    public async Task ReceiveMessagesAsync(string message,string groupName)
    {
        var publishId = GuidUtil.StringToGuid(groupName);
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(publishId);
        await publishingAgent.PublishEventAsync(new ReceiveMessageEvent()
        {
            Message = message
        });
    }

    public async Task SetGroupsAsync()
    {
        var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
        var telegramAgent = _clusterClient.GetGrain<ITelegramGAgent>(Guid.NewGuid());
        await telegramAgent.RegisterTelegramAsync("Test", "");
        await groupAgent.RegisterAsync(telegramAgent);

        var autogenAgent = _clusterClient.GetGrain<IAutogenGAgent>(Guid.NewGuid());
        await groupAgent.RegisterAsync(autogenAgent);
        int voterCount = 7;
        List<string> descriptions = new List<string>()
        {
            "You are a swimmer,",
            "You are an esports enthusiast.",
            "You are a truck driver.",
            "You are a basketball player.",
            "You are a girl who loves beauty.",
            "You are a singer.",
            "You are a boxer."
        };
        for (var i = 0; i < voterCount; i++)
        {
            var voteAgent = _clusterClient.GetGrain<IVoterGAgent>(Guid.NewGuid());
            await voteAgent.SetAgent($"Vote:{i}",
                $"You are a voter,and {descriptions[i]}. Based on a proposal, provide a conclusion of agreement or disagreement and give reasons.");
            await groupAgent.RegisterAsync(voteAgent);
        }

        await autogenAgent.RegisterAgentEvent("Vote",
            "Vote on the user's multiple options or preferences and explain the reason.",
            [typeof(VoterGEvent)]);

        var conclusionAgent = _clusterClient.GetGrain<IConclusionGAgent>(Guid.NewGuid());
        await conclusionAgent.SetAgent("Conclusion",
            "I'm a  Summarizer, When I collect 7 votes, I will summarize the 7 votes and then send the information to Telegram.");
        await conclusionAgent.SetVoteCount(voterCount);
        await groupAgent.RegisterAsync(conclusionAgent);

        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(PublishId);
        await publishingAgent.PublishToAsync(groupAgent);

        await publishingAgent.PublishEventAsync(new RequestAllSubscriptionsEvent());
    }
}