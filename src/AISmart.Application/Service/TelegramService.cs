using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Agent;
using AISmart.Agents;
using AISmart.Agents.Developer;
using AISmart.Agents.Group;
using AISmart.Agents.ImplementationAgent.Events;
using AISmart.Agents.Investment;
using AISmart.Agents.MarketLeader;
using AISmart.Agents.MarketLeader.Events;
using AISmart.Application.Grains.Agents.Developer;
using AISmart.Application.Grains.Agents.Investment;
using AISmart.Application.Grains.Agents.MarketLeader;
using AISmart.Common;
using AISmart.Events;
using AISmart.GAgent.Autogen;
using AiSmart.GAgent.SocialAgent.GAgent;
using AiSmart.GAgent.TestAgent;
using AiSmart.GAgent.TestAgent.ConclusionAgent;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;
using AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;
using AiSmart.GAgent.TestAgent.NamingContest.RankingAgent;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AiSmart.GAgent.TestAgent.NLPAgent;
using AiSmart.GAgent.TestAgent.Voter;
using AISmart.Options;
using AISmart.Sender;
using AISmart.Telegram;
using AISmart.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Orleans;
using Volo.Abp.Application.Services;

namespace AISmart.Service;

public class TelegramService : ApplicationService, ITelegramService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<TelegramService> _logger;
    private readonly TelegramTestOptions _telegramTestOptions;
    private readonly TelegramOptions _telegramOptions;

    public TelegramService(IOptions<TelegramTestOptions> telegramTestOptions, IOptions<TelegramOptions> telegramOption,
        IClusterClient clusterClient,
        ILogger<TelegramService> logger)
    {
        _clusterClient = clusterClient;
        _telegramTestOptions = telegramTestOptions.Value;
        _logger = logger;
        _telegramOptions = telegramOption.Value;
    }

    public async Task ReceiveMessagesAsync(TelegramUpdateDto updateMessage, StringValues token)
    {
        // await SetGroupsAsync();
        // To filter only messages that mention the bot, check if message.Entities.type == "mention".
        // Group message auto-reply, just add the bot as a group admin.
        _logger.LogInformation("IPublishingGAgent {token}",token);
        {
            if (NeedReply(updateMessage, token))
            {
                var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(GuidUtil.StringToGuid(token));
                await publishingAgent.PublishEventAsync(new ReceiveMessageEvent
                {
                    MessageId = updateMessage.Message.MessageId.ToString(),
                    ChatId = updateMessage.Message.Chat.Id.ToString(),
                    Message = updateMessage.Message.Text
                });
            }
        }
    }

    private bool NeedReply(TelegramUpdateDto updateMessage, StringValues token)
    {
        if (updateMessage.Message == null)
        {
            return false;
        }

        // Check if the chat type is private or if there's a mention of our bot in the message.
        if (updateMessage.Message.Chat.Type == "private")
        {
            return true;
        }

        // If the message contains entities, check for mentions.
        if (updateMessage.Message.Entities == null)
        {
            return false;
        }

        // Look for a mention that matches the token and decide accordingly.
        foreach (var entity in updateMessage.Message.Entities)
        {
            if (entity.Type == "mention")
            {
                var mentionText = updateMessage.Message.Text.Substring(entity.Offset, entity.Length);
                if (mentionText.Equals("@" + token, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public async Task SetGroupsAsyncForTelegram()
    {
        var groupId = GuidUtil.StringToGuid("Test");
        var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
        var telegramAgent = _clusterClient.GetGrain<ITelegramGAgent>(Guid.NewGuid());
        await telegramAgent.RegisterTelegramAsync("Test", "");
        var developerAgent = _clusterClient.GetGrain<IStateGAgent<DeveloperAgentState>>(Guid.NewGuid());
        var investmentAgent = _clusterClient.GetGrain<IStateGAgent<InvestmentAgentState>>(Guid.NewGuid());
        var marketLeaderAgent = _clusterClient.GetGrain<IStateGAgent<MarketLeaderAgentState>>(Guid.NewGuid());
        var autogenAgent = _clusterClient.GetGrain<IAutogenGAgent>(Guid.NewGuid());
        autogenAgent.RegisterAgentEvent(typeof(TelegramGAgent),
            [typeof(ReceiveMessageEvent), typeof(SendMessageEvent)]);
        autogenAgent.RegisterAgentEvent(typeof(DeveloperGAgent), [typeof(ImplementationEvent)]);
        autogenAgent.RegisterAgentEvent(typeof(InvestmentGAgent), [typeof(InvestmentEvent)]);
        autogenAgent.RegisterAgentEvent(typeof(MarketLeaderGAgent), [typeof(SocialEvent)]);

        await groupAgent.RegisterAsync(telegramAgent);
        await groupAgent.RegisterAsync(autogenAgent);
        await groupAgent.RegisterAsync(developerAgent);
        await groupAgent.RegisterAsync(investmentAgent);
        await groupAgent.RegisterAsync(marketLeaderAgent);

        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(groupId);
        await publishingAgent.PublishToAsync(groupAgent);

        await publishingAgent.PublishEventAsync(new RequestAllSubscriptionsEvent());
    }

    public async Task SetGroupsAsync()
    {
        var groupId = GuidUtil.StringToGuid("Test");
        var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
        var telegramAgent = _clusterClient.GetGrain<ITelegramGAgent>(Guid.NewGuid());
        await telegramAgent.RegisterTelegramAsync("Test", "");
        await groupAgent.RegisterAsync(telegramAgent);

        // var autogenAgent = _clusterClient.GetGrain<IAutogenGAgent>(Guid.NewGuid());
        // await groupAgent.Register(autogenAgent);
        int voterCount = 7;
        List<string> descriptions = new List<string>()
        {
            "You are a swimmer,",
            "You are an esports enthusiast.",
            "You are a truck driver.",
            "You are a basketball player.",
            "You are a girl who loves beauty.",
            "You are a singer.",
            "You are a boxer."
        };
        for (var i = 0; i < voterCount; i++)
        {
            var voteAgent = _clusterClient.GetGrain<IVoterGAgent>(Guid.NewGuid());
            await voteAgent.SetAgent($"Vote:{i}",
                $"You are a voter,and {descriptions[i]}. Based on a proposal, provide a conclusion of agreement or disagreement and give reasons.");
            await groupAgent.RegisterAsync(voteAgent);
        }

        // var chatAgent = _clusterClient.GetGrain<IChatGAgent>(Guid.NewGuid());
        // await chatAgent.SetAgent("TelegramChatBot", "I am a Telegram chat bot.");
        // await autogenAgent.RegisterAgentEvent("TelegramChatBot", "I am a Telegram chat bot.", [typeof(ChatGEvent)]);
        // await groupAgent.Register(chatAgent);

        // await autogenAgent.RegisterAgentEvent("Vote",
        //     "Vote on the user's multiple options or preferences and explain the reason.",
        //     [typeof(VoterGEvent)]);

        var nlpAgent = _clusterClient.GetGrain<INLPGAgent>(Guid.NewGuid());
        var nlpDescription = """
                             You are an NLP Bot. You need to determine whether the user's input is related to making choices. 
                             If the topic is related to making choices, please enhance the user's input and list the options available, then output them to the user. 
                             If the user's input is unrelated to making choices, please return "Error".
                             """;
        await nlpAgent.SetAgent("NlpAgent", nlpDescription);
        await groupAgent.RegisterAsync(nlpAgent);

        var conclusionAgent = _clusterClient.GetGrain<IConclusionGAgent>(Guid.NewGuid());
        await conclusionAgent.SetAgent("Conclusion",
            "you are a Summarizer, When you collect 7 votes, compile statistics on the voting and draw a conclusion..");
        await conclusionAgent.SetVoteCount(voterCount);
        await groupAgent.RegisterAsync(conclusionAgent);

        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(groupId);
        await publishingAgent.PublishToAsync(groupAgent);

        await publishingAgent.PublishEventAsync(new RequestAllSubscriptionsEvent());
    }

    public async Task RegisterBotAsync(RegisterTelegramDto registerTelegramDto)
    {
        var groupId = GuidUtil.StringToGuid(registerTelegramDto.BotName);
        var socialAgent = _clusterClient.GetGrain<ISocialGAgent>(groupId);
        await socialAgent.SetAgent(registerTelegramDto.BotName, "You need to answer all the questions you know.");
        var telegramAgent = _clusterClient.GetGrain<ITelegramGAgent>(groupId);
        await telegramAgent.RegisterTelegramAsync(registerTelegramDto.BotName, registerTelegramDto.Token);
        var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(groupId);
        await groupAgent.RegisterAsync(telegramAgent);
        await groupAgent.RegisterAsync(socialAgent);
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(groupId);
        await publishingAgent.PublishToAsync(groupAgent);
    }

    public async Task UnRegisterBotAsync(UnRegisterTelegramDto unRegisterTelegramDto)
    {
        var groupId = GuidUtil.StringToGuid(unRegisterTelegramDto.BotName);
        var telegramAgent = _clusterClient.GetGrain<ITelegramGAgent>(groupId);
        await telegramAgent.UnRegisterTelegramAsync(unRegisterTelegramDto.BotName);
        var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(groupId);
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(groupId);
        await publishingAgent.UnpublishFromAsync(groupAgent);
    }

    public async Task SetNamingGroupAsync()
    {
        var creativeCount = 10;

        List<string> descriptions = new List<string>()
        {
            "You are a generous swimmer,",
            "You are a introverted esports enthusiast.",
            "You are a optimistic truck driver.",
            "You are a pessimistic basketball player.",
            "You are a diligent girl who loves beauty.",
            "You are a lazy singer.",
            "You are a friendly boxer.",
            "You are a confident Accountant.",
            "You are a shy Teacher.",
            "You are a generous Lawyer."
        };

        // var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(GuidUtil.StringToGuid("NamingGroup"));
        var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
        // var trafficAgent = _clusterClient.GetGrain<ITrafficGAgent>(GuidUtil.StringToGuid("Traffic"));
        var trafficAgent = _clusterClient.GetGrain<ITrafficGAgent>(Guid.NewGuid());
        await trafficAgent.SetAgent("Traffic",
            "You need to determine whether the user's input is a question about naming. If it is, please return 'True'; otherwise, return 'False'.");
        await groupAgent.RegisterAsync(trafficAgent);
        var random = new Random();
        for (var i = 0; i < creativeCount; i++)
        {
            var temperature = random.NextDouble();
            var creativeName = $"creative-{i}";
            // var creativeAgent = _clusterClient.GetGrain<ICreativeGAgent>(GuidUtil.StringToGuid(creativeName));
            var creativeAgent = _clusterClient.GetGrain<ICreativeGAgent>(Guid.NewGuid());
            await creativeAgent.SetAgentWithTemperatureAsync(creativeName,
                $"{descriptions[i]} You must provide a definite name for the user's input regarding a naming question, without including any additional information.",
                (float)temperature);
            await trafficAgent.AddCreativeAgent(creativeAgent.GetPrimaryKey());

            await groupAgent.RegisterAsync(creativeAgent);
        }

        var telegramAgent = _clusterClient.GetGrain<ITelegramGAgent>(Guid.NewGuid());
        var aesCipher = new AESCipher(_telegramOptions.EncryptionPassword);
        var encryptToken = aesCipher.Encrypt(_telegramTestOptions.Token);
        await telegramAgent.RegisterTelegramAsync(_telegramTestOptions.BotName, encryptToken);
        await groupAgent.RegisterAsync(telegramAgent);

        // var judgeAgent = _clusterClient.GetGrain<IJudgeGAgent>(GuidUtil.StringToGuid("Judge"));
        var judgeAgent = _clusterClient.GetGrain<IJudgeGAgent>(Guid.NewGuid());
        await judgeAgent.SetAgent("Judge",
            "You are a rater, and you need to score the user's input naming based on the naming question. The score range is 0-100, different names should have different scores. Please provide the score directly, without including any additional information.");
        await groupAgent.RegisterAsync(judgeAgent);

        // var rankingAgent = _clusterClient.GetGrain<IJudgeGAgent>(GuidUtil.StringToGuid("Ranking"));
        var rankingAgent = _clusterClient.GetGrain<IRankingGAgent>(Guid.NewGuid());
        await groupAgent.RegisterAsync(rankingAgent);

        var publishId = GuidUtil.StringToGuid("TelegramNamingContestDemo");
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(publishId);
        await publishingAgent.PublishToAsync(groupAgent);
    }
}