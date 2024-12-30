using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Application.Grains;
using AISmart.Dapr;
using AISmart.Events;
using AISmart.GAgent.Core;
using AISmart.GEvents.MicroAI;
using AISmart.Grains;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;

namespace AISmart.Agent;

[Description("micro AI")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public abstract class MicroAIGAgent : GAgentBase<MicroAIGAgentState, AIMessageGEvent>, IMicroAIGAgent
{
    protected readonly ILogger<MicroAIGAgent> _logger;

    public MicroAIGAgent(ILogger<MicroAIGAgent> logger) : base(logger)
    {
        _logger = logger;
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult(
            "Represents an agent responsible for informing other agents when a micro AI thread is published.");
    }


    public async Task SetAgent(string agentName, string agentResponsibility)
    {
        RaiseEvent(new AISetAgentMessageGEvent
        {
            AgentName = agentName,
            AgentResponsibility = agentResponsibility
        });
        await ConfirmEvents();
        


        IChatAgentGrain chatAgentGrain = GrainFactory.GetGrain<IChatAgentGrain>(agentName);
        await chatAgentGrain.SetAgentAsync(agentResponsibility);

        
        var agentGuid = this.GetPrimaryKey();
        var streamId = StreamId.Create(CommonConstants.StreamNamespace, agentGuid);
        var stream = StreamProvider.GetStream<MicroAIMessage>(streamId);
        await stream.SubscribeAsync(HandlerMicroAIMessage);
    }

    protected async Task HandlerMicroAIMessage(MicroAIMessage items, StreamSequenceToken token)
    {
           
    }


    public async Task SetAgentWithTemperatureAsync(string agentName, string agentResponsibility, float temperature,
        int? seed = null,
        int? maxTokens = null)
    {
        RaiseEvent(new AISetAgentMessageGEvent
        {
            AgentName = agentName,
            AgentResponsibility = agentResponsibility
        });
        await ConfirmEvents();
        await GrainFactory.GetGrain<IChatAgentGrain>(agentName)
            .SetAgentWithTemperature(agentResponsibility, temperature, seed, maxTokens);
    }

    public async Task<MicroAIGAgentState> GetAgentState()
    {
        return State;
    }
}

public interface IMicroAIGAgent : IStateGAgent<MicroAIGAgentState>
{
    Task SetAgent(string agentName, string agentResponsibility);

    Task SetAgentWithTemperatureAsync(string agentName, string agentResponsibility, float temperature, int? seed = null,
        int? maxTokens = null);

    Task<MicroAIGAgentState> GetAgentState();
}