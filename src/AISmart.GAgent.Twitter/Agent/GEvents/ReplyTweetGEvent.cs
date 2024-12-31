using Orleans;

namespace AISmart.Agent.GEvents;

[GenerateSerializer]
public class ReplyTweetGEvent : TweetGEvent
{
    [Id(0)] public string TweetId { get; set; }
    [Id(1)] public string Text { get; set; }
}