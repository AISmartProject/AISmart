using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.GAgent.Core;
using AISmart.GAgents.Tests.TestEvents;
using AISmart.GAgents.Tests.TestGEvents;
using Microsoft.Extensions.Logging;

namespace AISmart.GAgents.Tests.TestGAgents;

[GenerateSerializer]
public class ChatTestGAgentState : StateBase
{
    [Id(0)]  public Dictionary<string, string> SendMessages { get; set; }
}

[GAgent]
public class ChatTestGAgent : GAgentBase<ChatTestGAgentState, MessageGEvent>
{
    public ChatTestGAgent(ILogger logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("This is a GAgent acts as a chatbot.");
    }

    public async Task HandleEventAsync(ReceiveMessageTestEvent eventData)
    {
        RaiseEvent(new ReceiveMessageTestGEvent
        {
            MessageId = eventData.MessageId,
            ChatId = eventData.ChatId,
            Message = eventData.Message,
            NeedReplyBotName = eventData.BotName
        });
        await ConfirmEvents();
        // await PublishAsync(new SocialTestEvent
        //     {
        //         MessageId = eventData.MessageId,
        //         Message = eventData.Message
        //     }
        //     .WithContext("ChatId", eventData.ChatId)
        //     .WithContext("GroupId", "Can't tell you.")
        //     .WithContext(new Dictionary<string, object?>
        //     {
        //         ["BotName"] = eventData.BotName
        //     }));
    }

    public async Task HandleEventAsync(SendMessageTestEvent eventData)
    {
        // Call the real API to send the message.
        if (State.SendMessages.IsNullOrEmpty())
        {
            State.SendMessages = [];
        }

        State.SendMessages.Add(eventData.ChatId, eventData.Message);
        await Task.CompletedTask;
    }
}