using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Agents.AutoGen;
using AISmart.Agents.Group;
using AISmart.Application.Grains;
using AISmart.Events;
using AISmart.GAgent.Autogen.EventSourcingEvent;
using AISmart.GAgent.Core;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AiSmart.GAgent.TestAgent.NamingContest.VoteAgent;
using AISmart.Grains;
using AISmart.Service;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans.Providers;

namespace AISmart.Agent;

[Description("Handle NamingContest")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class PumpFunPumpFunNamingContestGAgent : GAgentBase<PumpFunNamingContestGAgentState, PumpFunNameContestSEvent>, IPumpFunNamingContestGAgent
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

    public Task InitGroupInfoAsync(IniNetWorkMessagePumpFunSEvent iniNetWorkMessageSEvent)
    {
        RaiseEvent(iniNetWorkMessageSEvent);
        base.ConfirmEvents();
        return Task.CompletedTask;
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult(
            "Represents an agent responsible for informing other agents when a PumpFun thread is published.");
    }
    

    [AllEventHandler]
    public async Task HandleRequestAllEventAsync(EventWrapperBase @event)
    {
        
        _logger.LogInformation("NamingContestGAgent HandleRequestAllEventAsync :" +
                               JsonConvert.SerializeObject(@event));
        var eventWrapper = @event as EventWrapper<EventBase>;
        if (eventWrapper?.Event != null)
        {
            if (eventWrapper.Event is NamingAILogEvent logEvent)
            {
                await GrainFactory.GetGrain<INamingContestGrain>("NamingContestGrain")
                    .SendMessageAsync(State.groupId,logEvent, State.CallBackUrl);
            }
            else if (eventWrapper.Event is NamingLogEvent namingLogEvent)
            {
                await GrainFactory.GetGrain<INamingContestGrain>("NamingContestGrain")
                    .SendMessageAsync(State.groupId,namingLogEvent, State.CallBackUrl);
            }

        }
    }

    [EventHandler]
    public async Task HandleRequestEventAsync(VoteCharmingCompleteEvent @event)
    {
        
        _logger.LogInformation("NamingContestGAgent HandleRequestEventAsync VoteCharmingCompleteEvent:" +
                               JsonConvert.SerializeObject(@event));

        await GrainFactory.GetGrain<INamingContestGrain>("NamingContestGrain")
            .SendMessageAsync(State.groupId,@event, State.CallBackUrl);
    }
    
    [EventHandler]
    public async Task HandleGroupCleanEventAsync(NamingLogEvent @event)
    {

        if (@event.Step == NamingContestStepEnum.HostSummaryComplete)
        {
            _logger.LogInformation("HandleGroupCleanEventAsync " +
                                   JsonConvert.SerializeObject(@event));
            var groupId = State.groupId;
            var groupGAgent = GrainFactory.GetGrain<IStateGAgent<GroupAgentState>>(groupId);
            List<GrainId> subGAgentIdList =  await groupGAgent.GetSubscribersAsync();
            _logger.LogInformation("HandleGroupCleanEventAsync subGAgentIdList" +
                                   JsonConvert.SerializeObject(subGAgentIdList));
            foreach (var grainId in subGAgentIdList)
            {
                var subGAgent = GrainFactory.GetGrain<IGAgent>(grainId);
                await groupGAgent.UnregisterAsync(subGAgent);

            }
            subGAgentIdList =  await groupGAgent.GetSubscribersAsync();
            _logger.LogInformation("HandleGroupCleanEventAsync subGAgentIdList end" +
                                   JsonConvert.SerializeObject(subGAgentIdList));
            _logger.LogInformation("HandleGroupCleanEventAsync end: " +
                                   JsonConvert.SerializeObject(@event));
        }
        
    }
}

public interface IPumpFunNamingContestGAgent : IStateGAgent<PumpFunNamingContestGAgentState>
{ 
    Task InitGroupInfoAsync(IniNetWorkMessagePumpFunSEvent iniNetWorkMessageSEvent);
 
}