using System.Buffers.Text;
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
    public async Task HandleEventAsync(DiscussionCompleteGEvent @event)
    {
        if (State.CurrentGrainId != @event.CreativeId)
        {
            Logger.LogError(
                $"Traffic DebatedCompleteGEvent Current GrainId not match {State.CurrentGrainId.ToString()}--{@event.CreativeId.ToString()}");
            return;
        }

        base.RaiseEvent(new AddChatHistorySEvent()
        {
            ChatMessage = new MicroAIMessage(Role.User.ToString(),
                AssembleMessageUtil.AssembleDiscussionContent(@event.CreativeName, @event.DiscussionReply))
        });

        base.RaiseEvent(new TrafficGrainCompleteSEvent()
        {
            CompleteGrainId = @event.CreativeId,
        });

        base.RaiseEvent(new DiscussionCountReduce());

        await base.ConfirmEvents();

        await DispatchCreativeDiscussion();
    }

    [EventHandler]
    public async Task HandleEventAsync(CreativeSummaryCompleteGEvent @event)
    {
        base.RaiseEvent(new AddChatHistorySEvent()
        {
            ChatMessage = new MicroAIMessage(Role.User.ToString(),
                AssembleMessageUtil.AssembleDiscussionSummary(@event.SummaryName, @event.SummaryName))
        });

        await base.ConfirmEvents();
        await PublishAsync(new NamingLogEvent(NamingContestStepEnum.JudgeStartAsking, Guid.Empty));

        await DispatchJudgeAgent();
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
            var summaryCreativeId = await SelectCreativeToSummary();
            await PublishAsync(new CreativeSummaryGEvent() { CreativeId = summaryCreativeId });
            //
            // await PublishAsync(new NamingLogEvent(NamingContestStepEnum.JudgeAsking, Guid.Empty));
            // await DispatchJudgeAgent();
            return;
        }

        var random = new Random();
        var randomId = random.Next(0, creativeList.Count());
        var selectCreative = creativeList[randomId];
        await PublishAsync(new DiscussionGEvent() { CreativeId = selectCreative.CreativeGrainId });

        await base.ConfirmEvents();
    }

    private async Task DispatchDebateAgent()
    {
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

    private async Task DispatchJudgeAgent()
    {
        var creativeList = State.JudgeAgentList.FindAll(f => State.CalledGrainIdList.Contains(f) == false).ToList();
        if (State.AskJudgeCount == 0)
        {
            // todo: score
            // await PublishAsync(new NamingLogEvent(NamingContestStepEnum.Complete, Guid.Empty));
            return;
        }

        var random = new Random();
        var index = random.Next(0, creativeList.Count);
        var selectedId = creativeList[index];
        RaiseEvent(new TrafficCallSelectGrainIdSEvent() { GrainId = selectedId });
        await base.ConfirmEvents();

        await PublishAsync(new JudgeAskingGEvent(){JudgeGuid = selectedId, History = State.ChatHistory});
    }

    public async Task DispatchCreativeToAnswer()
    {
        
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