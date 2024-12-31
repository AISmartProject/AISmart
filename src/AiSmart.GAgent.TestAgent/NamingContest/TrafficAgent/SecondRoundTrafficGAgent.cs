using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Events;
using AISmart.GAgent.Core;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;
using AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;
using AISmart.Grains;
using AutoGen.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

        List<Tuple<string, string>> creativeNaming = new List<Tuple<string, string>>();
        foreach (var creativeInfo in State.CreativeList)
        {
            creativeNaming.Add(new Tuple<string, string>(creativeInfo.CreativeName, creativeInfo.Naming));
        }

        await PublishAsync(new GroupChatStartGEvent()
            { IfFirstStep = false, ThemeDescribe = @event.Message, CreativeNameings = creativeNaming });

        await PublishAsync(new NamingLogEvent(NamingContestStepEnum.DiscussionStart, Guid.Empty));

        await GenerateDiscussionCount();
        
        await DispatchCreativeDiscussion();

        await base.ConfirmEvents();
    }

    [EventHandler]
    public async Task HandleEventAsync(NamedCompleteGEvent @event)
    {
        if (State.CurrentGrainId != @event.GrainGuid)
        {
            Logger.LogError(
                $"Traffic NamedCompleteGEvent Current GrainId not match {State.CurrentGrainId.ToString()}--{@event.GrainGuid.ToString()}");
            return;
        }

        base.RaiseEvent(new TrafficGrainCompleteGEvent()
        {
            CompleteGrainId = @event.GrainGuid,
        });

        base.RaiseEvent(new AddChatHistorySEvent()
        {
            ChatMessage = new MicroAIMessage(Role.User.ToString(),
                AssembleMessageUtil.AssembleNamingContent(@event.CreativeName, @event.NamingReply))
        });

        base.RaiseEvent(new CreativeNamingSEvent() { CreativeId = @event.GrainGuid, Naming = @event.NamingReply });

        await base.ConfirmEvents();

        await DispatchCreativeDiscussion();
    }

    [EventHandler]
    public async Task HandleEventAsync(DebatedCompleteGEvent @event)
    {
        if (State.CurrentGrainId != @event.GrainGuid)
        {
            Logger.LogError(
                $"Traffic DebatedCompleteGEvent Current GrainId not match {State.CurrentGrainId.ToString()}--{@event.GrainGuid.ToString()}");
            return;
        }

        base.RaiseEvent(new AddChatHistorySEvent()
        {
            ChatMessage = new MicroAIMessage(Role.User.ToString(),
                AssembleMessageUtil.AssembleDebateContent(@event.CreativeName, @event.DebateReply))
        });

        base.RaiseEvent(new TrafficGrainCompleteGEvent()
        {
            CompleteGrainId = @event.GrainGuid,
        });

        await base.ConfirmEvents();

        await DispatchDebateAgent();
    }

    [EventHandler]
    public async Task HandleEventAsync(JudgeVoteResultGEvent @event)
    {
        if (State.CurrentGrainId != @event.JudgeGrainId)
        {
            Logger.LogError(
                $"Traffic HandleEventAsync Current GrainId not match {State.CurrentGrainId.ToString()}--{@event.JudgeGrainId.ToString()}");
            return;
        }

        var creativeInfo = State.CreativeList.FirstOrDefault(f => f.Naming == @event.VoteName);
        if (creativeInfo != null)
        {
            var voteInfoStr = JsonConvert.SerializeObject(new JudgeVoteInfo()
            {
                AgentId = creativeInfo.CreativeGrainId, AgentName = creativeInfo.CreativeName,
                Nameing = @event.VoteName, Reason = @event.Reason
            });

            await PublishAsync(new NamingLogEvent(NamingContestStepEnum.JudgeVote, @event.JudgeGrainId,
                NamingRoleType.Judge, @event.JudgeName, voteInfoStr));
        }

        base.RaiseEvent(new TrafficGrainCompleteGEvent()
        {
            CompleteGrainId = @event.JudgeGrainId,
        });

        await base.ConfirmEvents();

        await DispatchJudgeAgent();
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
        var creativeList = State.CreativeList.FindAll(f => State.CalledGrainIdList.Contains(f.CreativeGrainId) == false)
            .ToList();
        if (creativeList.Count == 0 && State.DiscussionCount == 0)
        {
            // todo: summary
            await PublishAsync(new NamingLogEvent(NamingContestStepEnum.JudgeAsking, Guid.Empty));
            await DispatchJudgeAgent();
            return;
        }

        if (creativeList.Count == 0 && State.DiscussionCount > 0)
        {
            creativeList = State.CreativeList;
            RaiseEvent(new ClearCalledGrainsSEvent());
        }

        var random = new Random();
        var randomId = random.Next(0, creativeList.Count());
        var selectCreative = creativeList[randomId];
        await PublishAsync(new DiscussionGEvent() { CreativeId = selectCreative.CreativeGrainId });
    }

    private async Task DispatchDebateAgent()
    {
       
    }

    private async Task DispatchJudgeAgent()
    {
        var creativeList = State.JudgeAgentList.FindAll(f => State.CalledGrainIdList.Contains(f) == false).ToList();
        if (creativeList.Count == 0)
        {
            await PublishAsync(new NamingLogEvent(NamingContestStepEnum.Complete, Guid.Empty));
            return;
        }

        var random = new Random();
        var index = random.Next(0, creativeList.Count);
        var selectedId = creativeList[index];
        RaiseEvent(new TrafficCallSelectGrainIdSEvent() { GrainId = selectedId });
        await base.ConfirmEvents();

        await PublishAsync(new JudgeVoteGEVent() { JudgeGrainId = selectedId, History = State.ChatHistory });
    }

    private Task GenerateDiscussionCount()
    {
        var random = new Random();
        var discussionCount = random.Next(State.CreativeList.Count * 3 / 2, State.CreativeList.Count * 2);
        RaiseEvent(new SetDiscussionSEvent() { DiscussionCount = discussionCount });
        
        return Task.CompletedTask;
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
        await base.ConfirmEvents();
    }

    public async Task AddJudgeAgent(Guid judgeGrainId)
    {
        RaiseEvent(new AddJudgeSEvent() { JudgeGrainId = judgeGrainId });
        await ConfirmEvents();
    }

    public async Task SetAskJudgeNumber(int judgeNum)
    {
        RaiseEvent(new SetAskingJudgeSEvent() { AskingJudgeCount = judgeNum });
        await ConfirmEvents();
    }
}