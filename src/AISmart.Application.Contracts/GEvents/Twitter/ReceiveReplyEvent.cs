using System.ComponentModel;
using AISmart.Agents;
using Orleans;

namespace AISmart.GEvents.Twitter;

[Description("Receive a reply from tweet.")]
[GenerateSerializer]
public class ReceiveReplyEvent : EventBase
{
    [Description("Unique identifier for the tweet which got replied.")]
    [Id(0)]  public string TweetId { get; set; }
    [Description("Text content of the reply.")]
    [Id(1)] public string Text { get; set; }
}
