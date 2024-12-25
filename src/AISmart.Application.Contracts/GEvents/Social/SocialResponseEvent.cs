using System.ComponentModel;
using AISmart.Agents;
using Orleans;

namespace AISmart.GEvents.Social;

[GenerateSerializer]
public class SocialResponseEvent:EventBase
{
    [Id(0)] public string ResponseContent { get; set; }
    [Description("Unique identifier for the target chat where the message will be sent.")]
    [Id(1)]  public string ChatId { get; set; }
    [Description("Optional ID of the message to which this message is a reply.")]
    [Id(2)]   public string? ReplyMessageId { get; set; }
}