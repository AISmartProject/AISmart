using System.ComponentModel;
using AISmart.Agents;
using Orleans;

namespace AISmart.GEvents.Social;

[Description("I can chat with users.")]
[GenerateSerializer]
public class SocialEvent:EventWithResponseBase<SocialResponseEvent>
{
    [Description("The content of the chat.")]
    [Id(0)] public string Content { get; set; }
    [Id(1)]  public string MessageId { get; set; }
    [Description("Unique identifier for the chat from which the message was received.")]
    [Id(2)]  public string ChatId { get; set; }
    [Id(3)]  public string TweetId { get; set; }
}