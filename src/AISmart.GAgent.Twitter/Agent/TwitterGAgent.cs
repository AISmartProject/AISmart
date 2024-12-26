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
        
        RaiseEvent(new ReceiveTweetReplyGEvent
        {
            TweetId = @event.TweetId,
            Text = @event.Text
        });
        await ConfirmEvents();
        await PublishAsync(new SocialEvent()
        {
            Content = @event.Text,
            TweetId = @event.TweetId
        });
    }
    
    [EventHandler]
    public async Task HandleEventAsync(CreateTweetEvent @event)
    {
        if (@event.Text.IsNullOrEmpty())
        {
            return;
        }
        
        // RaiseEvent(new CreateTweetGEvent()
        // {
        //     Text = @event.Text,
        // });
        // await ConfirmEvents();
        
        _logger.LogDebug("Create Tweet: " + @event.Text);
        await PublishAsync(new SocialEvent()
        {
            Content = @event.Text
        });
    }
    
    [EventHandler]
    public async Task HandleEventAsync(SocialResponseEvent @event)
    {
        // RaiseEvent(new ReplyTweetGEvent()
        // {
        //     Text = @event.ResponseContent,
        //     TweetId = @event.TweetId
        // });
        // await ConfirmEvents();
        
        _logger.LogDebug("SocialResponse for tweet Message: " + @event.ResponseContent);
        await GrainFactory.GetGrain<ITwitterGrain>(State.UserId).CreateTweetAsync(
            @event.ResponseContent, State.Token, State.TokenSecret);
    }
    
    public async Task BindTwitterAccount(string userId, string token, string tokenSecret)
    {
        RaiseEvent(new BindTwitterAccountEvent()
        {
            UserId = userId,
            Token = token,
            TokenSecret = tokenSecret
        });
        await ConfirmEvents();
    }
    
    public async Task UnbindTwitterAccount()
    {
        RaiseEvent(new UnbindTwitterAccountEvent()
        {
        });
        await ConfirmEvents();
    }
}

public interface ITwitterGAgent : IStateGAgent<TwitterGAgentState>
{
    Task BindTwitterAccount(string userId, string token, string tokenSecret);
    Task UnbindTwitterAccount();
}