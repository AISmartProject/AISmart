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

public class FirstRoundTrafficGAgent : GAgentBase<FirstTrafficState, TrafficEventSourcingBase>, IFirstTrafficGAgent
{
    public FirstRoundTrafficGAgent(ILogger<MicroAIGAgent> logger) : base(logger)
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
        
        await DispatchCreativeAgent();
    }
    
    [EventHandler]
    public async Task HandleEventAsync(DebatedCompleteGEvent @event)
    {
        if (State.CurrentCreativeId != @event.GrainGuid)
        {
            Logger.LogError("Traffic TrafficInformCreativeEvent");
            return;
        }

        base.RaiseEvent(new TrafficDebateCompleteGEvent()
        {
            CompleteGrainId = @event.GrainGuid,
        });

        await base.ConfirmEvents();

        await DispatchDebateAgent();
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
        // the first stage - Naming
        var creativeList = State.CreativeList.FindAll(f => State.CalledCreativeList.Contains(f) == false).ToList();
        if (creativeList.Count == 0)
        {
            // end message 
            await PublishAsync(new TrafficNamingContestOver() { NamingQuestion = State.NamingContent });

            // begin the second stage debate
            await DispatchDebateAgent();
        }
        else
        {
            // random select one Agent
            var random = new Random();
            var index = random.Next(0, creativeList.Count);
            var selectedId = creativeList[index];
            RaiseEvent(new TrafficCallSelectCreativeSEvent() { CreativeGrainId = selectedId });
            await base.ConfirmEvents();

            // route to the selectedId Agent 
            await PublishAsync(new TrafficInformCreativeGEvent()
                { NamingContent = State.NamingContent, CreativeGrainId = selectedId });
        }
    }

    private async Task DispatchDebateAgent()
    {
        // the second stage - debate
        var creativeList = State.CreativeList.ToList();

        
        int count = State.DebateStageCount;

        if (count == 0)
        {
            await PublishAsync(new TrafficDebateOver() { NamingQuestion = State.NamingContent });
            
            // begin the third stage judge
            await DispatchJudgeAgent();
        }
        else
        {
            // random select one Agent
            var random = new Random();
            var index = random.Next(0, creativeList.Count);
            var selectedId = creativeList[index];
            RaiseEvent(new TrafficCallSelectCreativeSEvent() { CreativeGrainId = selectedId });
            await base.ConfirmEvents();

            // route to the selectedId Agent 
            await PublishAsync(new TrafficInformDebateGEvent()
                { NamingContent = State.NamingContent, CreativeGrainId = selectedId });
        }
    }

    private async Task DispatchJudgeAgent()
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

    public async Task AddCreativeAgent(Guid creativeGrainId)
    {
        RaiseEvent(new AddCreativeAgent() { CreativeGrainId = creativeGrainId });
        await base.ConfirmEvents();
    }

    public Task AddJudgeAgent(Guid creativeGrainId)
    {
        throw new NotImplementedException();
    }
}