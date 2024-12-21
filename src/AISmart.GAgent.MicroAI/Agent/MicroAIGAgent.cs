using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Application.Grains;
using AISmart.Events;
using AISmart.GEvents.MicroAI;
using AISmart.Grains;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.Agent;

[Description("micro AI")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public abstract class MicroAIGAgent<GEvent,GEventResponse> : GAgentBase<MicroAIGAgentState, AIMessageGEvent>, IMicroAIGAgent
{
    protected readonly ILogger<MicroAIGAgent<GEvent,GEventResponse>> _logger;

    public MicroAIGAgent(ILogger<MicroAIGAgent<GEvent,GEventResponse>> logger) : base(logger)
    {
        _logger = logger;
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult(
            "Represents an agent responsible for informing other agents when a micro AI thread is published.");
    }


    [EventHandler]
    public abstract Task<GEventResponse> HandleEventAsync(GEvent @event);

    public async Task SetAgent(string agentName, string agentResponsibility)
    {
        RaiseEvent(new AISetAgentMessageGEvent
        {
            AgentName = agentName,
            AgentResponsibility = agentResponsibility
        });
        await ConfirmEvents();
        await GrainFactory.GetGrain<IChatAgentGrain>(agentName).SetAgentAsync(agentResponsibility);
    }

    public async Task<MicroAIGAgentState> GetAgentState()
    {
        return State;
    }
}

public interface IMicroAIGAgent: IStateGAgent<MicroAIGAgentState>
{
    Task SetAgent(string agentName, string agentResponsibility);
    
    Task<MicroAIGAgentState> GetAgentState();
}