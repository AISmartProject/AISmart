using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.GAgent.Core;
using AISmart.GEvents.Social;
using AISmart.GEvents.Twitter;
using AISmart.Grains;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.Agent;

[Description("Handle telegram")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class TwitterGAgent : GAgentBase<TwitterGAgentState, TweetGEvent>, ITwitterGAgent
{
    private readonly ILogger<TwitterGAgent> _logger;

    public TwitterGAgent(ILogger<TwitterGAgent> logger) : base(logger)
    {
        _logger = logger;
    }
    
    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult(
            "Represents an agent responsible for informing other agents when a twitter thread is published.");
    }
    
    [EventHandler]
    public async Task HandleEventAsync(ReceiveReplyEvent @event)
    {
        _logger.LogInformation("Tweet ReceiveReplyEvent " + @event.TweetId);
        await PublishAsync(new SocialEvent()
        {
            Content = @event.Text,
            MessageId = @event.TweetId
        });
    }
    
    [EventHandler]
    public async Task HandleEventAsync(CreateTweetEvent @event)
    {
        _logger.LogDebug("HandleEventAsync CreateTweetEvent, text: {}",  @event.Text);
        if (@event.Text.IsNullOrEmpty())
        {
            return;
        }
        
        if (State.UserId.IsNullOrEmpty())
        {
            return;
        }
        
        await PublishAsync(new SocialEvent()
        {
            Content = @event.Text
        });
    }
    
    [EventHandler]
    public async Task HandleEventAsync(SocialResponseEvent @event)
    {
        _logger.LogDebug("HandleEventAsync SocialResponseEvent, content: {}, id: {id}",  
            @event.ResponseContent, @event.ReplyMessageId);
        if (@event.ReplyMessageId.IsNullOrEmpty())
        {
            await GrainFactory.GetGrain<ITwitterGrain>(State.UserId).CreateTweetAsync(
                @event.ResponseContent, State.Token, State.TokenSecret);
        }
        else
        {
            RaiseEvent(new ReplyTweetGEvent
            {
                TweetId = @event.ReplyMessageId,
                Text = @event.ResponseContent
            });
            await ConfirmEvents();
            
            await GrainFactory.GetGrain<ITwitterGrain>(State.UserId).ReplyTweetAsync(
                @event.ResponseContent, @event.ReplyMessageId, State.Token, State.TokenSecret);
        }
    }
    
    [EventHandler]
    public async Task HandleEventAsync(ReplyMentionEvent @event)
    {
        _logger.LogDebug("HandleEventAsync ReplyMentionEvent");
        if (State.UserId.IsNullOrEmpty())
        {
            return;
        }
        
        var mentionTweets = await GrainFactory.GetGrain<ITwitterGrain>(State.UserId).GetRecentMentionAsync(State.UserName);
        _logger.LogDebug("HandleEventAsync GetRecentMentionAsync, count: {cnt}", mentionTweets.Count);
        foreach (var tweet in mentionTweets)
        {
            if (!State.RepliedTweets.Keys.Contains(tweet.Id))
            {
                await PublishAsync(new SocialEvent()
                {
                    Content = tweet.Text,
                    MessageId = tweet.Id
                });
            }
        }
    }
    
    public async Task BindTwitterAccount(string userName, string userId, string token, string tokenSecret)
    {
        _logger.LogDebug("HandleEventAsync BindTwitterAccount，userId: {userId}, userName: {userName}", 
            userId, userName);
        RaiseEvent(new BindTwitterAccountGEvent()
        {
            UserId = userId,
            Token = token,
            TokenSecret = tokenSecret,
            UserName = userName
        });
        await ConfirmEvents();
    }
    
    public async Task UnbindTwitterAccount()
    {
        _logger.LogDebug("HandleEventAsync UnbindTwitterAccount，userId: {userId}", State.UserId);
        RaiseEvent(new UnbindTwitterAccountEvent()
        {
        });
        await ConfirmEvents();
    }
}

public interface ITwitterGAgent : IStateGAgent<TwitterGAgentState>
{
    Task BindTwitterAccount(string userName, string userId, string token, string tokenSecret);
    Task UnbindTwitterAccount();
}