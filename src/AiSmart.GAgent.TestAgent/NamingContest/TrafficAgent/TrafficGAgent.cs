using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Events;
using AISmart.GAgent.Core;
using AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;
using AISmart.Grains;
using Microsoft.Extensions.Logging;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

public class TrafficGAgent : GAgentBase<TrafficState, TrafficEventSourcingBase>, ITrafficGAgent
{
    public TrafficGAgent(ILogger<MicroAIGAgent> logger) : base(logger)
    {
    }

    [EventHandler]
    public async Task HandleEventAsync(ReceiveMessageEvent @event)
    {
        var message = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
            .SendAsync(@event.Message, new List<MicroAIMessage>());
        if (message.Content("True"))
        {
            RaiseEvent(new TrafficNameStartSEvent { Content = @event.Message });
            await base.ConfirmEvents();

            // todo:call creative agent
        }
    }

    [EventHandler]
    public async Task HandleEventAsync(NamedCompleteGEvent @event)
    {
        if (State.CurrentCreativeId != @event.GrainGuid)
        {
            Logger.LogError("Traffic TrafficInformCreativeEvent");
        }

        base.RaiseEvent(new TrafficCreativeFinishSEvent()
        {
            CreativeGrainId = @event.GrainGuid,
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
        await LoadSubscribersAsync();
        foreach (var grainId in _subscribers.State)
        {
            
        }
        ;
        // todo:call next creative agent
        // todo:call judge creative agent
        // todo: when over raise TrafficNamingContestOver event
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