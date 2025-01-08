using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AISmart.Agent.NamingContestTelegram;
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
using AISmart.GAgent.Telegram.Agent;
using AISmart.GAgent.Telegram.Dtos;
using AISmart.GAgent.Telegram.Options;
using AiSmart.GAgent.TestAgent.ConclusionAgent;
using AiSmart.GAgent.TestAgent.LoadTestAgent;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;
using AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AiSmart.GAgent.TestAgent.NamingContest.VoteAgent;
using AiSmart.GAgent.TestAgent.NLPAgent;
using AiSmart.GAgent.TestAgent.Voter;
using AISmart.Sender;
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
    private static List<ICreativeGAgent> _creativeList = new List<ICreativeGAgent>();
    private static List<IJudgeGAgent> _judgeList = new List<IJudgeGAgent>();
    private static List<IFirstTrafficGAgent> _firstTrafficList = new List<IFirstTrafficGAgent>();
    private static List<IPublishingGAgent> _firstStepPublishingList = new List<IPublishingGAgent>();
    private static List<ISecondTrafficGAgent> _secondTrafficList = new List<ISecondTrafficGAgent>();

    private static Dictionary<string, ILoadTestGAgentCount> _loadTestRecords =
        new Dictionary<string, ILoadTestGAgentCount>();

    private static Dictionary<string, IPublishingGAgent> _publishingList = new Dictionary<string, IPublishingGAgent>();

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
        _logger.LogInformation("IPublishingGAgent {token}", token);
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
        await publishingAgent.RegisterAsync(groupAgent);

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
        await publishingAgent.RegisterAsync(groupAgent);

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
        await publishingAgent.RegisterAsync(groupAgent);
    }

    public async Task UnRegisterBotAsync(UnRegisterTelegramDto unRegisterTelegramDto)
    {
        var groupId = GuidUtil.StringToGuid(unRegisterTelegramDto.BotName);
        var telegramAgent = _clusterClient.GetGrain<ITelegramGAgent>(groupId);
        await telegramAgent.UnRegisterTelegramAsync(unRegisterTelegramDto.BotName);
        var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(groupId);
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(groupId);
        await publishingAgent.UnregisterAsync(groupAgent);
    }

    public async Task SetNamingGroupAsync(string groupName)
    {
        var creativeCount = 2;
        var judgeCount = 7;

        List<Tuple<string, string>> creativeDescriptions = new List<Tuple<String, string>>()
        {
            new Tuple<string, string>("Emma Carter",
                "Emma Carter is a 29-year-old software engineer specializing in AI and machine learning. She holds a master’s degree in Computer Science from Stanford University and has worked at several leading tech companies, including a role as a lead developer at a prominent startup.\n\nEmma is known for her innovative solutions in natural language processing and has contributed to open-source projects in the AI community. In her free time, she enjoys painting, hiking, and volunteering as a mentor for aspiring women in tech. Emma is passionate about using technology to address social challenges and is currently working on a project that leverages AI to improve access to education in underserved communities."),
            new Tuple<string, string>("Lucas Bennett",
                "Lucas Bennett is a 35-year-old architect and urban planner known for designing sustainable and eco-friendly buildings. He graduated with honors from the Massachusetts Institute of Technology (MIT) with a degree in Architecture and Urban Studies.\n\nLucas has led projects in various countries, focusing on integrating green technology into urban spaces. His designs often prioritize renewable energy, efficient water usage, and maximizing natural light. He is a frequent speaker at international architecture conferences and has published articles on the future of urban sustainability.\n\nOutside of work, Lucas enjoys photography, cycling, and experimenting with 3D printing to create prototypes for his designs. He is also an advocate for affordable housing and works with local communities to design accessible living spaces.")
        };

        List<Tuple<string, string>> judgeDescriptions = new List<Tuple<String, string>>()
        {
            new Tuple<string, string>("Olivia Harper",
                "Olivia is a 28-year-old data scientist who specializes in predictive analytics. She works for a fintech company, where she develops algorithms to optimize investment strategies. Olivia enjoys solving complex problems and is an advocate for ethical AI in business. In her free time, she loves rock climbing and writing poetry."),
            new Tuple<string, string>("Ethan Blake",
                "Ethan is a 40-year-old chef and restaurant owner known for his innovative fusion cuisine. His flagship restaurant, \"Harvest Flame,\" has received multiple culinary awards. Outside the kitchen, Ethan is a food blogger and often conducts workshops on sustainable cooking"),
            new Tuple<string, string>("Sophia Martinez",
                "Sophia, a 34-year-old marine biologist, spends her days researching coral reef ecosystems. She has worked on global conservation projects and often writes articles to raise awareness about ocean preservation. Sophia is also a certified scuba diver and a passionate photographer."),
            new Tuple<string, string>("Liam Brooks",
                "Liam is a 25-year-old indie game developer who creates story-driven games with a focus on mental health awareness. His recent game, \"Echoed Mind,\" gained critical acclaim for its narrative depth. Liam enjoys drawing, composing music, and mentoring aspiring game designers"),
            new Tuple<string, string>("Amelia Ross",
                "Amelia is a 42-year-old investigative journalist who has exposed several high-profile corruption cases. She works for an international news agency and frequently travels to conflict zones. Amelia is a recipient of the Pulitzer Prize and a strong advocate for press freedom."),
            new Tuple<string, string>("Noah Clarke",
                "Noah is a 31-year-old environmental engineer who specializes in renewable energy solutions. He has led projects to build wind farms and solar power grids in developing regions. Noah enjoys hiking, birdwatching, and conducting community workshops on sustainable living."),
            new Tuple<string, string>("Mia Chen",
                "Mia is a 23-year-old fashion designer with a flair for creating eco-friendly clothing. Her brand, \"Green Threads,\" uses only sustainable materials and is popular among environmentally conscious consumers. Mia also runs an online platform teaching DIY fashion techniques."),
            new Tuple<string, string>("Benjamin Hayes",
                "Benjamin is a 45-year-old astrophysicist who works at a leading space research center. He has contributed to groundbreaking research on black holes and exoplanets. Benjamin is an engaging science communicator who frequently appears on TV and podcasts."),
            new Tuple<string, string>("Charlotte Price",
                " Charlotte is a 30-year-old entrepreneur who founded a successful wellness app called \"Mindful Moments.\" The app combines meditation, therapy resources, and fitness routines to help users manage stress. Charlotte is also a yoga instructor and a motivational speaker."),
        };

        // var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(GuidUtil.StringToGuid("NamingGroup"));
        var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
        // var trafficAgent = _clusterClient.GetGrain<ITrafficGAgent>(GuidUtil.StringToGuid("Traffic"));
        var trafficAgent = _clusterClient.GetGrain<IFirstTrafficGAgent>(Guid.NewGuid());
        await trafficAgent.SetAgent("Traffic",
            "traffic");
        await groupAgent.RegisterAsync(trafficAgent);
        var random = new Random();
        for (var i = 0; i < creativeCount; i++)
        {
            var creativeInfo = creativeDescriptions[i];
            var temperature = random.NextDouble();
            // var creativeAgent = _clusterClient.GetGrain<ICreativeGAgent>(GuidUtil.StringToGuid(creativeName));
            var creativeAgent = _clusterClient.GetGrain<ICreativeGAgent>(Guid.NewGuid());
            await creativeAgent.SetAgentWithTemperatureAsync(creativeInfo.Item1, creativeInfo.Item2,
                (float)temperature);
            await trafficAgent.AddCreativeAgent(creativeInfo.Item1, creativeAgent.GetPrimaryKey());

            await groupAgent.RegisterAsync(creativeAgent);
        }

        for (var i = 0; i < judgeCount; i++)
        {
            var judgeInfo = judgeDescriptions[i];
            var judgeAgent = _clusterClient.GetGrain<IJudgeGAgent>(Guid.NewGuid());
            await judgeAgent.SetAgent(judgeInfo.Item1, judgeInfo.Item2);
            await trafficAgent.AddJudgeAgent(judgeAgent.GetPrimaryKey());
            await groupAgent.RegisterAsync(judgeAgent);
        }

        var telegramAgent = _clusterClient.GetGrain<ITelegramGAgent>(Guid.NewGuid());
        var aesCipher = new AESCipher(_telegramOptions.EncryptionPassword);
        var encryptToken = aesCipher.Encrypt(_telegramTestOptions.Token);
        await telegramAgent.RegisterTelegramAsync(_telegramTestOptions.BotName, encryptToken);
        await groupAgent.RegisterAsync(telegramAgent);

        // var namingTelegram = _clusterClient.GetGrain<INamingContestTelegramGAgent>(Guid.NewGuid());
        // await namingTelegram.SetAgent("namingTelegram","You need to determine whether the user's input is a question about naming. If it is, please return 'True'; otherwise, return 'False'.");
        // await groupAgent.RegisterAsync(namingTelegram);

        var loadTestGAgent = _clusterClient.GetGrain<ILoadTestGAgentCount>(Guid.NewGuid());
        await groupAgent.RegisterAsync(loadTestGAgent);
        RecordLoadTest(groupName, loadTestGAgent);
        _logger.LogInformation("Current LoadTestRecords: {LoadTestRecords}",
            string.Join(", ", _loadTestRecords.Select(kv => $"{kv.Key}: {kv.Value}")));

        //
        // var rankingAgent = _clusterClient.GetGrain<IRankingGAgent>(Guid.NewGuid());
        // await groupAgent.RegisterAsync(rankingAgent);

        var publishId = GuidUtil.StringToGuid(groupName);
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(publishId);
        await publishingAgent.RegisterAsync(groupAgent);
        _publishingList.Add(groupName, publishingAgent);
    }

    private void RecordLoadTest(string groupName, ILoadTestGAgentCount agent)
    {
        _logger.LogInformation($"Recording load test for group: {groupName}");
        _loadTestRecords[groupName] = agent;
    }

    public async Task<LoadTestMessageCountResult> GetLoadTestMessageCount(string groupName)
    {
        if (string.IsNullOrEmpty(groupName))
        {
            return new LoadTestMessageCountResult
            {
                Success = false,
                Message = "Group name cannot be null or empty."
            };
        }

        if (!_loadTestRecords.TryGetValue(groupName, out var agent) || agent == null)
        {
            _logger.LogInformation($"No agent found for group: {groupName}");
            return new LoadTestMessageCountResult
            {
                Success = false,
                Message = $"No agent found for group: {groupName}"
            };
        }

        (int Number, DateTime StartTimestamp, DateTime EndTimestamp) agentInfo;
        try
        {
            agentInfo = await agent.GetLoadTestGAgentInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the agent count.");
            return new LoadTestMessageCountResult
            {
                Success = false,
                Message = "An error occurred while retrieving the agent count."
            };
        }

        return new LoadTestMessageCountResult
        {
            Success = true,
            AgentCount = agentInfo.Number,
            StartTime = agentInfo.StartTimestamp,
            EndTime = agentInfo.EndTimestamp,
            Duration = agentInfo.EndTimestamp - agentInfo.StartTimestamp,
            Message = $"Successfully retrieved agent count for group: {groupName}"
        };
    }

    public Task SendMessageToAllGroup(string message)
    {
        foreach (var item in _publishingList)
        {
            item.Value.PublishEventAsync(new GroupStartEvent() { Message = message });
        }

        return Task.CompletedTask;
    }

    public async Task InitGroupListAsync()
    {
        var creativeCount = 2;
        var judgeCount = 1;
        List<Tuple<string, string>> creativeDescriptions = new List<Tuple<String, string>>()
        {
            new Tuple<string, string>("Emma Carter",
                "Emma Carter is a 29-year-old software engineer specializing in AI and machine learning. She holds a master’s degree in Computer Science from Stanford University and has worked at several leading tech companies, including a role as a lead developer at a prominent startup.\n\nEmma is known for her innovative solutions in natural language processing and has contributed to open-source projects in the AI community. In her free time, she enjoys painting, hiking, and volunteering as a mentor for aspiring women in tech. Emma is passionate about using technology to address social challenges and is currently working on a project that leverages AI to improve access to education in underserved communities."),
            new Tuple<string, string>("Lucas Bennett",
                "Lucas Bennett is a 35-year-old architect and urban planner known for designing sustainable and eco-friendly buildings. He graduated with honors from the Massachusetts Institute of Technology (MIT) with a degree in Architecture and Urban Studies.\n\nLucas has led projects in various countries, focusing on integrating green technology into urban spaces. His designs often prioritize renewable energy, efficient water usage, and maximizing natural light. He is a frequent speaker at international architecture conferences and has published articles on the future of urban sustainability.\n\nOutside of work, Lucas enjoys photography, cycling, and experimenting with 3D printing to create prototypes for his designs. He is also an advocate for affordable housing and works with local communities to design accessible living spaces."),
            new Tuple<string, string>("Elena Varga",
                "Elena Varga is a passionate wildlife conservationist who has dedicated her career to protecting endangered species and restoring their natural habitats. She has worked on projects in Africa and Southeast Asia, specializing in elephant and tiger conservation. Elena holds a master's degree in Environmental Science and frequently collaborates with local communities to develop sustainable solutions that benefit both wildlife and humans. In her free time, she enjoys hiking, photography, and volunteering for environmental education programs."),
            new Tuple<string, string>("Marcus Delaney",
                "Marcus Delaney is an AI ethicist working to ensure that artificial intelligence technologies are developed responsibly and equitably. With a Ph.D. in Philosophy and a focus on ethics, Marcus advises tech companies on creating transparent and bias-free AI systems. He has written several papers on the intersection of AI, privacy, and human rights. Outside his professional life, Marcus enjoys playing jazz piano, attending philosophy book clubs, and mentoring students interested in tech ethics.")
        };

        List<Tuple<string, string>> judgeDescriptions = new List<Tuple<String, string>>()
        {
            new Tuple<string, string>("Olivia Harper",
                "Olivia is a 28-year-old data scientist who specializes in predictive analytics. She works for a fintech company, where she develops algorithms to optimize investment strategies. Olivia enjoys solving complex problems and is an advocate for ethical AI in business. In her free time, she loves rock climbing and writing poetry."),
            new Tuple<string, string>("Ethan Blake",
                "Ethan is a 40-year-old chef and restaurant owner known for his innovative fusion cuisine. His flagship restaurant, \"Harvest Flame,\" has received multiple culinary awards. Outside the kitchen, Ethan is a food blogger and often conducts workshops on sustainable cooking"),
            new Tuple<string, string>("Sophia Martinez",
                "Sophia, a 34-year-old marine biologist, spends her days researching coral reef ecosystems. She has worked on global conservation projects and often writes articles to raise awareness about ocean preservation. Sophia is also a certified scuba diver and a passionate photographer."),
            new Tuple<string, string>("Liam Brooks",
                "Liam is a 25-year-old indie game developer who creates story-driven games with a focus on mental health awareness. His recent game, \"Echoed Mind,\" gained critical acclaim for its narrative depth. Liam enjoys drawing, composing music, and mentoring aspiring game designers"),
            new Tuple<string, string>("Amelia Ross",
                "Amelia is a 42-year-old investigative journalist who has exposed several high-profile corruption cases. She works for an international news agency and frequently travels to conflict zones. Amelia is a recipient of the Pulitzer Prize and a strong advocate for press freedom."),
            new Tuple<string, string>("Noah Clarke",
                "Noah is a 31-year-old environmental engineer who specializes in renewable energy solutions. He has led projects to build wind farms and solar power grids in developing regions. Noah enjoys hiking, birdwatching, and conducting community workshops on sustainable living."),
            new Tuple<string, string>("Mia Chen",
                "Mia is a 23-year-old fashion designer with a flair for creating eco-friendly clothing. Her brand, \"Green Threads,\" uses only sustainable materials and is popular among environmentally conscious consumers. Mia also runs an online platform teaching DIY fashion techniques."),
            new Tuple<string, string>("Benjamin Hayes",
                "Benjamin is a 45-year-old astrophysicist who works at a leading space research center. He has contributed to groundbreaking research on black holes and exoplanets. Benjamin is an engaging science communicator who frequently appears on TV and podcasts."),
            new Tuple<string, string>("Charlotte Price",
                " Charlotte is a 30-year-old entrepreneur who founded a successful wellness app called \"Mindful Moments.\" The app combines meditation, therapy resources, and fitness routines to help users manage stress. Charlotte is also a yoga instructor and a motivational speaker."),
        };

        var random = new Random();
        for (var i = 0; i < creativeCount; i++)
        {
            var creativeInfo = creativeDescriptions[i];
            var temperature = random.NextDouble();
            // var creativeAgent = _clusterClient.GetGrain<ICreativeGAgent>(GuidUtil.StringToGuid(creativeName));
            var creativeAgent = _clusterClient.GetGrain<ICreativeGAgent>(Guid.NewGuid());
            await creativeAgent.SetAgentWithTemperatureAsync(creativeInfo.Item1, creativeInfo.Item2,
                (float)temperature);
            _creativeList.Add(creativeAgent);
        }

        for (var i = 0; i < judgeCount; i++)
        {
            var judgeInfo = judgeDescriptions[i];
            var judgeAgent = _clusterClient.GetGrain<IJudgeGAgent>(Guid.NewGuid());
            await judgeAgent.SetAgent(judgeInfo.Item1, judgeInfo.Item2);
            _judgeList.Add(judgeAgent);
        }
    }

    public async Task StartFirstRoundTestAsync(string message)
    {
        var groupList = new List<IStateGAgent<GroupAgentState>>();
        var groupCount = 1;
        for (var i = 0; i < groupCount; i++)
        {
            var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
            groupList.Add(groupAgent);

            var traffic = _clusterClient.GetGrain<IFirstTrafficGAgent>(Guid.NewGuid());
            _firstTrafficList.Add(traffic);
            await groupAgent.RegisterAsync(traffic);

            var publishAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
            await publishAgent.RegisterAsync(groupAgent);
            _firstStepPublishingList.Add(publishAgent);
        }

        for (var i = 0; i < _creativeList.Count; i++)
        {
            var creative = _creativeList[i];
            await groupList[i % groupCount].RegisterAsync(creative);
            await _firstTrafficList[i % groupCount]
                .AddCreativeAgent(await creative.GetCreativeName(), creative.GetPrimaryKey());
        }

        for (var i = 0; i < _judgeList.Count; i++)
        {
            var judge = _judgeList[i];
            await groupList[i % groupCount].RegisterAsync(judge);
            await _firstTrafficList[i % groupCount].AddJudgeAgent(judge.GetPrimaryKey());
        }

        for (var i = 0; i < groupCount; i++)
        {
            var groupAgent = _firstStepPublishingList[i];
            await groupAgent.PublishEventAsync(new GroupStartEvent() { Message = message });
        }

        var creativeJudgeAgentList = new List<Guid>();
        creativeJudgeAgentList.AddRange(_creativeList.Select(x => x.GetPrimaryKey()).ToList());
        var voteJudgeAgentList = new List<Guid>();
        voteJudgeAgentList.AddRange(_judgeList.Select(x => x.GetPrimaryKey()).ToList());
        
        var round = 1;
        IVoteCharmingGAgent voteCharmingGAgent = _clusterClient.GetGrain<IVoteCharmingGAgent>(GuidUtil.GetVoteCharmingGrainId(round.ToString()));
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
        await publishingAgent.RegisterAsync(voteCharmingGAgent);

        await publishingAgent.PublishEventAsync(new InitVoteCharmingEvent()
        {
            JudgeGuidList = voteJudgeAgentList,
            CreativeGuidList = creativeJudgeAgentList,
            Round = Convert.ToInt32(round),
            TotalBatches = 2
        });
    }

    public async Task StartSecondRoundTestAsync(string message)
    {
        var groupCount = 2;
        var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
        var secondTraffic = _clusterClient.GetGrain<ISecondTrafficGAgent>(Guid.NewGuid());
        await secondTraffic.SetAgent("secondTraffic", "");
        await secondTraffic.SetAskJudgeNumber(2);
        await secondTraffic.SetRoundNumber(2);
        
        await groupAgent.RegisterAsync(secondTraffic);
        for (var i = 0; i < groupCount; i++)
        {
            var creative = _creativeList[i * groupCount];
            await groupAgent.RegisterAsync(creative);

            await secondTraffic.AddCreativeAgent(await creative.GetCreativeName(), creative.GetPrimaryKey());
        }

        foreach (var judge in _judgeList)
        {
            await groupAgent.RegisterAsync(judge);
            await secondTraffic.AddJudgeAgent(judge.GetPrimaryKey());
        }

        var publishAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
        await publishAgent.RegisterAsync(groupAgent);
        await publishAgent.PublishEventAsync(new GroupStartEvent() { Message = message });
    }
}