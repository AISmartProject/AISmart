using System;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.GAgent.Core;
using AISmart.GEvents.Social;
using AISmart.GEvents.Twitter;
using AISmart.Grains;
using Microsoft.Extensions.Logging;

namespace AISmart.Agent;

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
        await GrainFactory.GetGrain<ITwitterGrain>(Guid.Parse(Guid.NewGuid().ToString())).CreateTweetAsync(
            @event.Text, State.AccountName);
    }
    
    [EventHandler]
    public async Task HandleEventAsync(SocialResponseEvent @event)
    {
        if (@event.TweetId.IsNullOrEmpty())
        {
            return;
        }
        
        RaiseEvent(new ReplyTweetGEvent()
        {
            Text = @event.ResponseContent,
            TweetId = @event.TweetId
        });
        await ConfirmEvents();
        
        _logger.LogDebug("SocialResponse for tweet Message: " + @event.ResponseContent);
        await GrainFactory.GetGrain<ITwitterGrain>(Guid.Parse(@event.TweetId)).ReplyTweetAsync(
            @event.ResponseContent, @event.TweetId);
    }
}

public interface ITwitterGAgent : IStateGAgent<TwitterGAgentState>
{
    // Task SetTelegramConfig( string botName,string token);
}