using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Events;
using AISmart.GAgent.Core;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;
using AISmart.Grains;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

public class SecondRoundTrafficGAgent : GAgentBase<TrafficState, TrafficEventSourcingBase>, ITrafficGAgent
{
    public SecondRoundTrafficGAgent(ILogger<MicroAIGAgent> logger) : base(logger)
    {
    }

    [EventHandler]
    public async Task HandleEventAsync(GroupStartEvent @event)
    {
        RaiseEvent(new TrafficNameStartSEvent { Content = @event.Message });
        await base.ConfirmEvents();
        await DispatchCreativeAgent();
    }

    [EventHandler]
    public async Task HandleEventAsync(NamedCompleteGEvent @event)
    {
        if (State.CurrentCreativeId != @event.GrainGuid)
        {
            Logger.LogError("Traffic TrafficInformCreativeEvent");
            return;
        }

        base.RaiseEvent(new TrafficCreativeCompleteGEvent()
        {
            CompleteGrainId = @event.GrainGuid,
        });

        await base.ConfirmEvents();
        await PublishAsync(new JudgeGEvent()
        {
            CreativeGrainId = @event.GrainGuid,
            CreativeName = @event.CreativeName,
            NamingReply = @event.NamingReply,
            NamingQuestion = State.NamingContent,
        });

        await DispatchCreativeAgent();
    }

    public Task<MicroAIGAgentState> GetStateAsync()
    {
        throw new NotImplementedException();
    }

    public override Task<string> GetDescriptionAsync()
    {
        throw new NotImplementedException();
    }

    private async Task DispatchCreativeAgent()
    {
        var creativeList = State.CreativeList.FindAll(f => State.CalledCreativeList.Contains(f) == false).ToList();
        if (creativeList.Count == 0)
        {
            await PublishAsync(new TrafficNamingContestOver() { NamingQuestion = State.NamingContent });
            return;
        }

        var random = new Random();
        var index = random.Next(0, creativeList.Count);
        var selectedId = creativeList[index];
        RaiseEvent(new TrafficCallSelectCreativeSEvent() { CreativeGrainId = selectedId });
        await base.ConfirmEvents();

        await PublishAsync(new TrafficInformCreativeGEvent()
            { NamingContent = State.NamingContent, CreativeGrainId = selectedId });
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

    public async Task SetAgentWithTemperatureAsync(string agentName, string agentResponsibility, float temperature, int? seed = null,
        int? maxTokens = null)
    {
        RaiseEvent(new TrafficSetAgentSEvent
        {
            AgentName = agentName,
            Description = agentResponsibility
        });
        await ConfirmEvents();

        await GrainFactory.GetGrain<IChatAgentGrain>(agentName).SetAgentWithTemperature(agentResponsibility, temperature, seed, maxTokens);
    }

    public Task<MicroAIGAgentState> GetAgentState()
    {
        throw new NotImplementedException();
    }

    public async Task AddCreativeAgent(Guid creativeGrainId)
    {
        RaiseEvent(new AddCreativeAgent() { CreativeGrainId = creativeGrainId });
        await base.ConfirmEvents();
    }
}