using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Agents.AutoGen;
using AISmart.Application.Grains;
using AISmart.Events;
using AISmart.GAgent.Autogen.EventSourcingEvent;
using AISmart.GAgent.Core;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.Grains;
using AISmart.Service;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans.Providers;

namespace AISmart.Agent;

[Description("Handle NamingContest")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class PumpFunPumpFunNamingContestGAgent : GAgentBase<PumpFunNamingContestGAgentState, PumpFunNameContestGEvent>, IPumpFunNamingContestGAgent
{
    private readonly ILogger<PumpFunPumpFunNamingContestGAgent> _logger;
    public PumpFunPumpFunNamingContestGAgent(ILogger<PumpFunPumpFunNamingContestGAgent> logger) : base(logger)
    {
        _logger = logger;
    }

    public Task<PumpFunPumpFunNamingContestGAgent> GetStateAsync()
    {
        throw new NotImplementedException();
    }

    public Task InitGroupInfoAsync(IniNetWorkMessagePumpFunGEvent iniNetWorkMessageGEvent)
    {
        RaiseEvent(iniNetWorkMessageGEvent);
        base.ConfirmEvents();
        return Task.CompletedTask;
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("Represents an agent responsible for informing other agents when a PumpFun thread is published.");
    }

    
    [EventHandler]
    public async Task HandleRequestAllSubscriptionsEventAsync(RequestAllSubscriptionsEvent @event)
    {
        _logger.LogInformation("NamingContestGAgent HandleRequestAllSubscriptionsEventAsync :" + JsonConvert.SerializeObject(@event));
        
    }
    
    [EventHandler]
    public async Task HandleRequestAllSubscriptionsEventAsync<T>(EventWrapper<T> @event)
    {
        _logger.LogInformation("NamingContestGAgent HandleRequestAllSubscriptionsEventAsync :" + JsonConvert.SerializeObject(@event));
        await GrainFactory.GetGrain<INamingContestGrain>("NamingContestGrain")
            .SendMessageAsync((@event.Event as NameContentGEvent)!,State.CallBackUrl);
    }
}

public interface IPumpFunNamingContestGAgent : IStateGAgent<PumpFunNamingContestGAgentState>
{ 
    Task InitGroupInfoAsync(IniNetWorkMessagePumpFunGEvent iniNetWorkMessageGEvent);

}