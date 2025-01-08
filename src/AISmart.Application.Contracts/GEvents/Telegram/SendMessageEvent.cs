using System.ComponentModel;
using AISmart.Agents;
using Orleans;

namespace AISmart.Events;

[Description("Send a message to telegram.")]
[GenerateSerializer]
public class SendMessageEvent :EventBase
{
    [Description("Unique identifier for the target chat where the message will be sent.")]
    [Id(0)]  public string ChatId { get; set; }
    [Description("Text content of the message to be sent.")]
    [Id(1)]  public string Message { get; set; }
    [Description("File path or URL of the photo to be sent with the message.")]
    [Id(2)]  public string Photo { get; set; }
    
    [Description("Optional caption for the photo, if provided.")]
    [Id(3)]  public string? Caption { get; set; }
    [Description("Optional ID of the message to which this message is a reply.")]
    [Id(4)]   public string? ReplyMessageId { get; set; }
    [Description("The name of the bot.")]
    [Id(5)]   public string BotName { get; set; }
}