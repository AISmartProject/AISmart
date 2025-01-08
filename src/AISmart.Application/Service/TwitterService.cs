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
    private const string GroupAgentName = "GroupAgent";
    private const string SocialAgentName = "SocialAgent";
    private const string PublishAgentName = "PublishAgent";
    private const string Role = "You are a Twitter user, please answer the question from that perspective and response the content of tweet directly!";

    public TwitterService(
        IClusterClient clusterClient,
        ILogger<TwitterService> logger)
    {
        _clusterClient = clusterClient;
        _logger = logger;
    }
    
    public async Task BindTwitterAccountAsync(BindTwitterAccountDto bindTwitterAccountDto)
    {
        var twitterAgent = _clusterClient.GetGrain<ITwitterGAgent>(GuidUtil.StringToGuid(bindTwitterAccountDto.UserId));
        var hasGroup = await twitterAgent.UserHasBoundAsync();
        _logger.LogInformation("BindTwitterAccountAsync，userId: {userId}, userName: {userName}, hasGroup:{hasGroup}", 
            bindTwitterAccountDto.UserId, bindTwitterAccountDto.UserName, hasGroup);
        if (hasGroup)
        {
            await twitterAgent.BindTwitterAccountAsync(bindTwitterAccountDto.UserName, bindTwitterAccountDto.UserId, bindTwitterAccountDto.Token, bindTwitterAccountDto.TokenSecret);
        }
        else
        {
            await twitterAgent.BindTwitterAccountAsync(bindTwitterAccountDto.UserName, bindTwitterAccountDto.UserId, bindTwitterAccountDto.Token, bindTwitterAccountDto.TokenSecret);
            var socialAgent = _clusterClient.GetGrain<ISocialGAgent>(GuidUtil.StringToGuid(bindTwitterAccountDto.UserId+SocialAgentName));
            await socialAgent.SetAgent(bindTwitterAccountDto.UserId, Role);
            var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(GuidUtil.StringToGuid(bindTwitterAccountDto.UserId+GroupAgentName));
            await groupAgent.RegisterAsync(twitterAgent);
            await groupAgent.RegisterAsync(socialAgent);
            var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(GuidUtil.StringToGuid(bindTwitterAccountDto.UserId+PublishAgentName));
            await publishingAgent.RegisterAsync(groupAgent);
        }
    }
    
    public async Task UnbindTwitterAccountAsync(UnbindTwitterAccountDto unbindTwitterAccountDto)
    {
        _logger.LogInformation("UnbindTwitterAccountAsync，userId: {userId}", unbindTwitterAccountDto.UserId);
        var groupId = GuidUtil.StringToGuid(unbindTwitterAccountDto.UserId);
        var twitterAgent = _clusterClient.GetGrain<ITwitterGAgent>(groupId);
        await twitterAgent.UnbindTwitterAccountAsync();
        var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(GuidUtil.StringToGuid(unbindTwitterAccountDto.UserId+GroupAgentName));
        var socialAgent = _clusterClient.GetGrain<ISocialGAgent>(GuidUtil.StringToGuid(unbindTwitterAccountDto.UserId+SocialAgentName));
        await groupAgent.UnregisterAsync(twitterAgent);
        await groupAgent.UnregisterAsync(socialAgent);
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(GuidUtil.StringToGuid(unbindTwitterAccountDto.UserId+PublishAgentName));
        await publishingAgent.UnregisterAsync(groupAgent);
    }

    public async Task PostTweetAsync(PostTweetDto postTweetDto)
    {
        _logger.LogInformation("PostTweetAsync，userId: {userId}", postTweetDto.UserId);
        var socialAgent = _clusterClient.GetGrain<ISocialGAgent>(GuidUtil.StringToGuid(postTweetDto.UserId+SocialAgentName));
        await socialAgent.SetAgent(postTweetDto.UserId, Role);
        
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(GuidUtil.StringToGuid(postTweetDto.UserId+PublishAgentName));
        await publishingAgent.PublishEventAsync(new CreateTweetEvent
        {
            Text = postTweetDto.Text
        });
    }
    
    public async Task ReplyMentionAsync(ReplyMentionDto replyMentionDto)
    {
        _logger.LogInformation("ReplyMentionAsync，userId: {userId}", replyMentionDto.UserId);
        var socialAgent = _clusterClient.GetGrain<ISocialGAgent>(GuidUtil.StringToGuid(replyMentionDto.UserId+SocialAgentName));
        await socialAgent.SetAgent(replyMentionDto.UserId, Role);
        
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(GuidUtil.StringToGuid(replyMentionDto.UserId+PublishAgentName));
        await publishingAgent.PublishEventAsync(new ReplyMentionEvent
        {
        });
    }
}