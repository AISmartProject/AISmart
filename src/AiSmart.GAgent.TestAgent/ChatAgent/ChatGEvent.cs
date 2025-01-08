using System.ComponentModel;
using AISmart.Agents;

namespace AiSmart.GAgent.TestAgent;

[Description("I can chat with users.")]
[GenerateSerializer]
public class ChatGEvent:EventWithResponseBase<ChatResponseGEvent>
{
    [Description("The content of the chat.")]
    [Id(0)] public string Content { get; set; }
}