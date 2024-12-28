using System;
using System.Threading.Tasks;
using AISmart.Agent;
using AISmart.Agents;
using AISmart.Agents.Group;
using AISmart.Common;
using AiSmart.GAgent.SocialAgent.GAgent;
using AISmart.GEvents.Twitter;
using AISmart.Sender;
using AISmart.Twitter;
using Microsoft.Extensions.Logging;
using Orleans;
using Volo.Abp.Application.Services;

namespace AISmart.Service;

public class TwitterService : ApplicationService, ITwitterService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<TwitterService> _logger;

    public TwitterService(
        IClusterClient clusterClient,
        ILogger<TwitterService> logger)
    {
        _clusterClient = clusterClient;
        _logger = logger;
    }
    
    public async Task BindTwitterAccountAsync(BindTwitterAccountDto bindTwitterAccountDto)
    {
        var groupId = GuidUtil.StringToGuid(bindTwitterAccountDto.UserId);
        var socialAgent = _clusterClient.GetGrain<ISocialGAgent>(groupId);
        await socialAgent.SetAgent(bindTwitterAccountDto.UserId, "You need to answer all the questions you know.");
        var twitterAgent = _clusterClient.GetGrain<ITwitterGAgent>(groupId);
        await twitterAgent.BindTwitterAccount(bindTwitterAccountDto.UserId, bindTwitterAccountDto.Token, bindTwitterAccountDto.TokenSecret);
        var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(groupId);
        await groupAgent.RegisterAsync(twitterAgent);
        await groupAgent.RegisterAsync(socialAgent);
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(groupId);
        await publishingAgent.PublishToAsync(groupAgent);
    }
    
    public async Task UnbindTwitterAccountAsync(UnbindTwitterAccountDto unbindTwitterAccountDto)
    {
        var groupId = GuidUtil.StringToGuid(unbindTwitterAccountDto.UserId);
        var twitterAgent = _clusterClient.GetGrain<ITwitterGAgent>(groupId);
        await twitterAgent.UnbindTwitterAccount();
        var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(groupId);
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(groupId);
        await publishingAgent.UnpublishFromAsync(groupAgent);
    }

    public async Task PostTweetAsync(PostTweetDto postTweetDto)
    {
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(GuidUtil.StringToGuid(postTweetDto.UserId));
        await publishingAgent.PublishEventAsync(new CreateTweetEvent
        {
            Text = postTweetDto.Text
        });
    }
    
    public async Task ReplyMentionAsync(ReplyMentionDto replyMentionDto)
    {
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(GuidUtil.StringToGuid(replyMentionDto.UserId));
        await publishingAgent.PublishEventAsync(new ReplyMentionEvent
        {
        });
    }
}