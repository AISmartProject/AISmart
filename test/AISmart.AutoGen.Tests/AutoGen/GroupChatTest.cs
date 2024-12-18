using AISmart.AutoGen.Tests.Common;
using AutoGen;
using AutoGen.Anthropic.DTO;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using Shouldly;

namespace AISmart.AutoGen.Tests;

public class GroupChatTest : AISmartAutoGenTestBase
{
    public GroupChatTest()
    {
    }

    [Fact]
    public async Task Group_Chat_Test()
    {
        var client = GetRequiredService<ChatClient>();

        #region agent defines

        IAgent groupAdmin = new OpenAIChatAgent(
                chatClient: client,
                name: "groupAdmin",
                systemMessage: "You are the admin of the group chat")
            .RegisterMessageConnector()
            .RegisterPrintMessage();

        IAgent adminAgent = new OpenAIChatAgent(chatClient: client, name: "admin", """
                You are a manager who solves user problems by organizing agents,
                Here's available agents who you can assign task to:
                - DexExchange: support crypto coin buy or sell or price from decentralized exchange
                - AelfBlockChain: handle aelf blockchain transaction
                - Twitter: handle  twitter request

                The workflow is as follows:
                - You take the problem from user.
                - You break the problem into small tasks, When a task is completed, you need to compare the task's outcome with the user's intent to determine whether the next task needs to be executed
                - The execution result of each task will serve as the context for the next task.
                - Based on the Agent's response, reorganize the next request in line with the user's intent until the user's issue is resolved.
                
                You can use the following json format to assign task to agents:
                ```task
                {
                    "to": "{agent_name}",
                    "task": "{a short description of the task}",
                    "context": "{previous context from scratchpad}"
                }
                ```
                
                If you need to ask user for extra information, you can use the following format:
                ```ask
                {
                    "question": "{question}"
                }
                ```
                """)
            .RegisterMessageConnector()
            .RegisterPrintMessage();

        #region AelfBlockChain Agent

        var aelfBlockChain = new AelfBlockChain();
        var aelfBlockChainFunctionMap = new Dictionary<string, Func<string, Task<string>>>()
        {
            { nameof(aelfBlockChain.BuyTokenAsync), aelfBlockChain.BuyTokenAsyncWrapper },
        };
        var aelfBlockChainMiddleware =
            new FunctionCallMiddleware(new[] { aelfBlockChain.BuyTokenAsyncContract }, aelfBlockChainFunctionMap);
        IAgent aelfBlockChainAgent =
            new OpenAIChatAgent(chatClient: client, name: "AelfBlockChain",
                    systemMessage:
                    "you are the aelf blockchain handler agent. you can send or receive transaction from aelf chain")
                .RegisterMessageConnector()
                .RegisterStreamingMiddleware(aelfBlockChainMiddleware)
                .RegisterPrintMessage();

        #endregion

        #region Twitter BlockChain Agent

        var twitter = new Twitter();
        var twitterContract = new List<FunctionContract>()
        {
            twitter.GetTwitterNewsContract,
            twitter.SendTwitterMessageContract
        };

        var functionMap = new Dictionary<string, Func<string, Task<string>>>()
        {
            { nameof(twitter.GetLastNews), twitter.GetLastNewsWrapper },
            { nameof(twitter.SendTwitterMessage), twitter.SendTwitterMessageWrapper }
        };

        var twitterMiddleware = new FunctionCallMiddleware(twitterContract, functionMap);
        IAgent twitterAgent = new OpenAIChatAgent(chatClient: client, name: "Twitter",
                systemMessage: "you are twitter AI assistant")
            .RegisterMessageConnector()
            .RegisterStreamingMiddleware(twitterMiddleware)
            .RegisterPrintMessage();

        #endregion

        #region Dex exchange Agent

        var dexExchange = new DexExchange();
        var dexExchangeContract = new List<FunctionContract>()
        {
            dexExchange.GetTokenPriceAsyncContract
        };

        var dexExchangeFunctionMap = new Dictionary<string, Func<string, Task<string>>>()
        {
            { nameof(dexExchange.GetTokenPriceAsync), dexExchange.GetTokenPriceAsyncWrapper }
        };

        var dexExchangeMiddleware = new FunctionCallMiddleware(dexExchangeContract, dexExchangeFunctionMap);
        IAgent dexAgent = new OpenAIChatAgent(chatClient: client, name: "DecentralizedExchange",
                systemMessage:
                """
                you are decentralized exchange AI assistant, you can get crypto coin price and buy or sell crypto coin.
                """)
            .RegisterMessageConnector()
            .RegisterStreamingMiddleware(dexExchangeMiddleware)
            .RegisterPrintMessage();

        IAgent userAgent = new DefaultReplyAgent(name: "user", defaultReply: GroupChatExtension.TERMINATE)
            .RegisterPrintMessage();

        #endregion

        #endregion

        #region agent transition define

        var adminToDexAgent = Transition.Create(adminAgent, dexAgent);
        var adminToTwitterAgent = Transition.Create(adminAgent, twitterAgent);
        var adminToAelfChainBlockAgent = Transition.Create(adminAgent, aelfBlockChainAgent);
        var telegramToAelfBlockChain = Transition.Create(twitterAgent, aelfBlockChainAgent);
        var dexToTwitterAgent = Transition.Create(dexAgent, twitterAgent);
        var dexToAelfBlockChain = Transition.Create(dexAgent, aelfBlockChainAgent);

        var userToAdmin = Transition.Create(userAgent, adminAgent);
        var adminToUser = Transition.Create(adminAgent, userAgent);

        var workflow = new Graph([
            adminToDexAgent, adminToTwitterAgent, adminToAelfChainBlockAgent, telegramToAelfBlockChain, userToAdmin,
            adminToUser, dexToTwitterAgent, dexToAelfBlockChain
        ]);

        #endregion

        #region group define

        var groupChat = new GroupChat(
            admin: groupAdmin,
            members: [adminAgent, dexAgent, aelfBlockChainAgent, userAgent, twitterAgent]
            // workflow: workflow
            );

        # endregion

        #region user call

        var task =
            """
            First, obtain the price of ELF-USDT from a decentralized exchange. 
            If the price is greater than $1, post a Twitter update about the ELF-USDT price and purchase ELF tokens from the Aelf blockchain
            """;
        var chatHistory = new List<IMessage>
        {
            new TextMessage(Role.User, task)
            {
                From = adminAgent.Name
            }
        };

        IAsyncEnumerable<IMessage> messageContent;
        IMessage? messageResponse = null;
        await foreach (var message in groupChat.SendAsync(chatHistory, maxRound: 10))
        {
            if (message.From == "DecentralizedExchange")
            {
                message.GetContent().ShouldBe("$2.0");
            }

            if (message.From == "Twitter")
            {
                message.GetContent().ShouldNotBeNull();
            }

            if (message.From == "AelfBlockChain")
            {
                message.GetContent().ShouldBe("get 100 million elf");
            }
        }
        
        #endregion
    }
}