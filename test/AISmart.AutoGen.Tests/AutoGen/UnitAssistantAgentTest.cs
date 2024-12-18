using AISmart.AutoGen.Tests.Common;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

namespace AISmart.AutoGen.Tests;

public class UnitAssistantAgent:AISmartAutoGenTestBase
{
    public UnitAssistantAgent()
    {
    }

    [Fact]
    public async Task AutoGen_Assistant_Agent_Test()
    {
        IAgent assistantAgent = new OpenAIChatAgent(
                chatClient: GetRequiredService<ChatClient>(),
                name: "assistant",
                systemMessage: "You convert what user said to all uppercase.")
            .RegisterMessageConnector()
            .RegisterPrintMessage();
        
        // talk to the assistant agent
        var reply = await assistantAgent.SendAsync("elf-usdt is 10$ !");
        reply.Should().BeOfType<TextMessage>();
        reply.GetContent().Should().Be("ELF-USDT IS 10$ !");

        // to carry on the conversation, pass the previous conversation history to the next call
        var conversationHistory = new List<IMessage>
        {
            new TextMessage(Role.User, "elf-usdt is 10$ !"), // first message
            reply, // reply from assistant agent
        };

        reply = await assistantAgent.SendAsync("fomo buy!!", conversationHistory);
        reply.Should().BeOfType<TextMessage>();
        reply.GetContent().Should().Be("FOMO BUY!!");
    }
}