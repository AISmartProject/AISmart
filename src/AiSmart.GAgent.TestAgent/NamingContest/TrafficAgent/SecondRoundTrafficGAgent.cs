using System.Buffers.Text;
using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.GAgent.Core;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;
using AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;
using AiSmart.GAgent.TestAgent.NamingContest.VoteAgent;
using AISmart.Grains;
using AISmart.Sender;
using AutoGen.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

public class SecondRoundTrafficGAgent : GAgentBase<SecondTrafficState, TrafficEventSourcingBase>, ISecondTrafficGAgent
{
    public SecondRoundTrafficGAgent(ILogger<MicroAIGAgent> logger) : base(logger)
    {
    }

    [EventHandler]
    public async Task HandleEventAsync(GroupStartEvent @event)
    {
        RaiseEvent(new TrafficNameStartSEvent { Content = @event.Message });
        RaiseEvent(new ChangeNamingStepSEvent { Step = NamingContestStepEnum.NamingStart });
        RaiseEvent(new AddChatHistorySEvent()
            { ChatMessage = new MicroAIMessage(Role.User.ToString(), @event.Message) });
        await PublishAsync(new NamingLogEvent(NamingContestStepEnum.NamingStart, Guid.Empty));
        
        List<Tuple<string, string>> creativeNaming = State.CreativeList.Select(creativeInfo =>
            new Tuple<string, string>(creativeInfo.CreativeName, creativeInfo.Naming)).ToList();

        await PublishAsync(new GroupChatStartGEvent()
            { IfFirstStep = false, ThemeDescribe = @event.Message, CreativeNameings = creativeNaming });

        await PublishAsync(new NamingLogEvent(NamingContestStepEnum.DiscussionStart, Guid.Empty));

        RaiseEvent(new ChangeNamingStepSEvent { Step = NamingContestStepEnum.DiscussionStart });
        await GenerateDiscussionCount();

        await ConfirmEvents();

        await DispatchCreativeDiscussion();
    }

    [EventHandler]
    public async Task HandleEventAsync(DiscussionCompleteGEvent @event)
    {
        if (State.CurrentGrainId != @event.CreativeId)
        {
            Logger.LogError(
                $"Traffic DebatedCompleteGEvent Current GrainId not match {State.CurrentGrainId.ToString()}--{@event.CreativeId.ToString()}");
            return;
        }

        RaiseEvent(new AddChatHistorySEvent()
        {
            ChatMessage = new MicroAIMessage(Role.User.ToString(),
                AssembleMessageUtil.AssembleDiscussionContent(@event.CreativeName, @event.DiscussionReply))
        });

        RaiseEvent(new TrafficGrainCompleteSEvent()
        {
            CompleteGrainId = @event.CreativeId,
        });

        RaiseEvent(new DiscussionCountReduceSEvent());

        await ConfirmEvents();

        await DispatchCreativeDiscussion();
    }

    [EventHandler]
    public async Task HandleEventAsync(CreativeSummaryCompleteGEvent @event)
    {
        RaiseEvent(new AddChatHistorySEvent()
        {
            ChatMessage = new MicroAIMessage(Role.User.ToString(),
                AssembleMessageUtil.AssembleDiscussionSummary(@event.SummaryName, @event.Reason))
        });
        
        await PublishAsync(new NamingLogEvent(NamingContestStepEnum.JudgeStartAsking, Guid.Empty));

        RaiseEvent(new ChangeNamingStepSEvent { Step = NamingContestStepEnum.JudgeAsking });
        await ConfirmEvents();
        
        await DispatchJudgeAsking();
    }

    [EventHandler]
    public async Task HandleEventAsync(JudgeAskingCompleteGEvent @event)
    {
        if (@event.JudgeGuid != State.CurrentGrainId)
        {
            Logger.LogError(
                $"Traffic JudgeAskingCompleteGEvent Current GrainId not match {State.CurrentGrainId.ToString()}--{@event.JudgeGuid.ToString()}");
            return;
        }

        RaiseEvent(new AddChatHistorySEvent()
        {
            ChatMessage = new MicroAIMessage(Role.User.ToString(),
                AssembleMessageUtil.AssembleJudgeAsking(@event.JudgeName, @event.Reply))
        });

        RaiseEvent(new TrafficGrainCompleteSEvent() { CompleteGrainId = @event.JudgeGuid });

        await ConfirmEvents();

        await DispatchCreativeToAnswer();
    }

    [EventHandler]
    public async Task HandleEventAsync(CreativeAnswerCompleteGEvent @event)
    {
        if (!@event.Answer.IsNullOrEmpty())
        {
            RaiseEvent(new AddChatHistorySEvent()
            {
                ChatMessage = new MicroAIMessage(Role.User.ToString(),
                    AssembleMessageUtil.AssembleCreativeAnswer(@event.CreativeName, @event.Answer))
            });
        }

        await ConfirmEvents();
        await DispatchJudgeAsking();
    }

    [EventHandler]
    public async Task HandleEventAsync(JudgeScoreCompleteGEvent @event)
    {
        RaiseEvent(new AddScoreJudgeCountSEvent());
        await ConfirmEvents();

        if (State.JudgeScoreCount == State.JudgeAgentList.Count)
        {
            await PublishAsync(new NamingLogEvent(NamingContestStepEnum.Complete, Guid.Empty));
            await PublishAsync(new NamingContestComplete());
            
            RaiseEvent(new ChangeNamingStepSEvent { Step = NamingContestStepEnum.Complete });
            
            await PublishMostCharmingEventAsync();
        }
    }
    
    [EventHandler]
    public async Task HandleEventAsync(HostSummaryCompleteGEvent @event)
    {
        if (State.CurrentGrainId != @event.HostId)
        {
            Logger.LogError(
                $"Traffic HandleEventAsync Current GrainId not match {State.CurrentGrainId.ToString()}--{@event.HostId.ToString()}");
            return;
        }

        base.RaiseEvent(new TrafficGrainCompleteSEvent()
        {
            CompleteGrainId = @event.HostId,
        });

        await base.ConfirmEvents();

        await DispatchHostAgent();
    }

    public Task<MicroAIGAgentState> GetStateAsync()
    {
        throw new NotImplementedException();
    }

    public override Task<string> GetDescriptionAsync()
    {
        throw new NotImplementedException();
    }

    private async Task DispatchCreativeDiscussion()
    {
        var random = new Random();
        var probility = random.Next(0, 10);
        var creativeList = new List<CreativeInfo>();
        if (probility > 7)
        {
            creativeList = State.CreativeList;
        }
        else
        {
            creativeList = State.CreativeList.FindAll(f => State.CalledGrainIdList.Contains(f.CreativeGrainId) == false)
                .ToList();
        }

        if (State.DiscussionCount == 0)
        {
            var summaryCreativeId = await SelectCreativeToSummary();
            await PublishAsync(new CreativeSummaryGEvent() { CreativeId = summaryCreativeId });

            RaiseEvent(new ChangeNamingStepSEvent { Step = NamingContestStepEnum.DiscussionSummary });
            RaiseEvent(new ClearCalledGrainsSEvent());
            return;
        }

        if (creativeList.Count == 0 && State.DiscussionCount > 0)
        {
            creativeList = State.CreativeList;
            RaiseEvent(new ClearCalledGrainsSEvent());
        }

        var randomId = random.Next(0, creativeList.Count());
        var selectCreative = creativeList[randomId];
        await PublishAsync(new DiscussionGEvent() { CreativeId = selectCreative.CreativeGrainId });
        RaiseEvent(new TrafficCallSelectGrainIdSEvent() { GrainId = selectCreative.CreativeGrainId });

        await ConfirmEvents();
    }

    private async Task PublishMostCharmingEventAsync()
    {
        IVoteCharmingGAgent voteCharmingGAgent =
            GrainFactory.GetGrain<IVoteCharmingGAgent>(Helper.GetVoteCharmingGrainId(NamingConstants.SecondRound));
        var publishingAgent = GrainFactory.GetGrain<IPublishingGAgent>(Guid.NewGuid());
        await publishingAgent.RegisterAsync(voteCharmingGAgent);

        await publishingAgent.PublishEventAsync(new VoteCharmingEvent()
        {
            AgentIdNameDictionary = State.CreativeList.ToDictionary(p => p.CreativeGrainId, p => p.CreativeName),
            Round = State.Round,
            VoteMessage = State.ChatHistory
        });
    }

    private async Task<Guid> SelectCreativeToSummary()
    {
        var result = Guid.Empty;
        try
        {
            var response = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
                .SendAsync(NamingConstants.TrafficSelectCreativePrompt, State.ChatHistory.ToList());
            if (response != null && response.Content.IsNullOrEmpty())
            {
                var creativeInfo = State.CreativeList.FirstOrDefault(f => f.CreativeName == response.Content);
                if (creativeInfo != null)
                {
                    result = creativeInfo.CreativeGrainId;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[SecondRoundTraffic] SelectCreativeToSummary error");
        }
        finally
        {
            if (result == Guid.Empty)
            {
                Random random = new Random();
                var index = random.Next(0, State.CreativeList.Count);

                result = State.CreativeList[index].CreativeGrainId;
            }
        }

        return result;
    }

    private Task GenerateDiscussionCount()
    {
        var random = new Random();
        var discussionCount = random.Next(State.CreativeList.Count * 3 / 2, State.CreativeList.Count * 2);
        RaiseEvent(new SetDiscussionSEvent() { DiscussionCount = discussionCount });

        return Task.CompletedTask;
    }

    private async Task DispatchJudgeAsking()
    {
        var creativeList = State.JudgeAgentList.FindAll(f => State.CalledGrainIdList.Contains(f) == false).ToList();
        if (State.AskJudgeCount == State.CalledGrainIdList.Count)
        {
            await PublishAsync(new NamingLogEvent(NamingContestStepEnum.JudgeStartScore, Guid.Empty));

            RaiseEvent(new ChangeNamingStepSEvent { Step = NamingContestStepEnum.JudgeScore });
            await PublishAsync(new JudgeScoreGEvent() { History = State.ChatHistory });
            return;
        }

        var random = new Random();
        var index = random.Next(0, creativeList.Count);
        var selectedId = creativeList[index];
        RaiseEvent(new TrafficCallSelectGrainIdSEvent() { GrainId = selectedId });
        await ConfirmEvents();

        await PublishAsync(new JudgeAskingGEvent() { JudgeGuid = selectedId, History = State.ChatHistory });
    }

    private async Task DispatchCreativeToAnswer()
    {
        var random = new Random();
        var index = random.Next(0, State.CreativeList.Count);
        var selectedCreative = State.CreativeList[index];

        await PublishAsync(new CreativeAnswerQuestionGEvent() { CreativeId = selectedCreative.CreativeGrainId });
    }
    
    private async Task DispatchHostAgent()
    {
        var hostAgentList = State.HostAgentList.FindAll(f => State.CalledGrainIdList.Contains(f) == false).ToList();
        if (hostAgentList.Count == 0)
        {
            await PublishAsync(new NamingLogEvent(NamingContestStepEnum.HostSummaryComplete, Guid.Empty));
            return;
        }

        var random = new Random();
        var index = random.Next(0, hostAgentList.Count);
        var selectedId = hostAgentList[index];
        RaiseEvent(new TrafficCallSelectGrainIdSEvent() { GrainId = selectedId });
        await base.ConfirmEvents();

        await PublishAsync(new HostSummaryGEvent() { HostId = selectedId, History = State.ChatHistory });
    }

    public async Task SetAgent(string agentName, string agentResponsibility)
    {
        RaiseEvent(new TrafficSetAgentSEvent
        {
            AgentName = agentName,
            Description = agentResponsibility
        });
        await ConfirmEvents();

        await GrainFactory.GetGrain<IChatAgentGrain>(agentName).SetAgentAsync(agentResponsibility);
    }

    public async Task SetAgentWithTemperatureAsync(string agentName, string agentResponsibility, float temperature,
        int? seed = null,
        int? maxTokens = null)
    {
        RaiseEvent(new TrafficSetAgentSEvent
        {
            AgentName = agentName,
            Description = agentResponsibility
        });
        await ConfirmEvents();

        await GrainFactory.GetGrain<IChatAgentGrain>(agentName)
            .SetAgentWithTemperature(agentResponsibility, temperature, seed, maxTokens);
    }

    public Task<MicroAIGAgentState> GetAgentState()
    {
        throw new NotImplementedException();
    }

    public async Task AddCreativeAgent(string creativeName, Guid creativeGrainId)
    {
        var creativeAgent = GrainFactory.GetGrain<ICreativeGAgent>(creativeGrainId);
        var naming = await creativeAgent.GetCreativeNaming();
        RaiseEvent(new AddCreativeAgent()
            { CreativeGrainId = creativeGrainId, CreativeName = creativeName, Naming = naming });
        await ConfirmEvents();
    }

    public async Task AddJudgeAgent(Guid judgeGrainId)
    {
        RaiseEvent(new AddJudgeSEvent() { JudgeGrainId = judgeGrainId });
        await ConfirmEvents();
    }

    public async Task AddHostAgent(Guid judgeGrainId)
    {
        RaiseEvent(new AddHostSEvent() { HostGrainId = judgeGrainId });
        await ConfirmEvents();
    }

    public async Task SetAskJudgeNumber(int judgeNum)
    {
        RaiseEvent(new SetAskingJudgeSEvent() { AskingJudgeCount = judgeNum });
        await ConfirmEvents();
    }

    public async Task SetRoundNumber(int round)
    {
        RaiseEvent(new SetRoundNumberSEvent() { RoundCount = round });
        await ConfirmEvents();
    }

    public Task<int> GetProcessStep()
    {
        return Task.FromResult((int)State.NamingStep);
    }
}