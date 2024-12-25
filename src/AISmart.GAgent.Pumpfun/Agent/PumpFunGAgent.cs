using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Agents.AutoGen;
using AISmart.Application.Grains;
using AISmart.Events;
using AISmart.GAgent.Core;
using AISmart.Grains;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans.Providers;

namespace AISmart.Agent;

[Description("Handle PumpFun")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class PumpFunGAgent : GAgentBase<PumpFunGAgentState, PumpFunMessageGEvent>, IPumpFunGAgent
{
    private readonly ILogger<PumpFunGAgent> _logger;
    public PumpFunGAgent(ILogger<PumpFunGAgent> logger) : base(logger)
    {
        _logger = logger;
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("Represents an agent responsible for informing other agents when a PumpFun thread is published.");
    }

    [EventHandler]
    public async Task HandleEventAsync(PumpFunReceiveMessageEvent @event)
    { 
        _logger.LogInformation("PumpFunReceiveMessageEvent:" + JsonConvert.SerializeObject(@event));
       RaiseEvent(new PumpFunReceiveMessageGEvent
       {
           ChatId = @event.ChatId,
           ReplyId = @event.ReplyId,
           RequestMessage = @event.RequestMessage
       });
       await ConfirmEvents();
       
       _logger.LogInformation("PumpFunReceiveMessageEvent2:" + JsonConvert.SerializeObject(@event));
       await PublishAsync(new AutoGenCreatedEvent
       {
           EventId = Guid.NewGuid(),
           Content = $"""
             Received a JSON-formatted message:{JsonConvert.SerializeObject(@event)}, The fields will be used in the final response except "RequestMessage".
             Please follow the process below.
             1. parse the message content, the fields in the JSON may be used in the final response..
             2. Please understand the content of the "RequestMessage" in the JSON format, process the response accordingly.
             3. Must pass the final result to the PumpFunSendMessageEvent of the PumpFunGAgent.
             """
       });
    }
    
    [EventHandler]
    public async Task HandleEventAsync(PumpFunSendMessageEvent @event)
    {
        _logger.LogInformation("PumpFunSendMessageEvent:" + JsonConvert.SerializeObject(@event));
        if (@event.ReplyId != null)
        {
            RaiseEvent(new PumpFunSendMessageGEvent()
            {
                ReplyId = @event.ReplyId,
                ReplyMessage = @event.ReplyMessage
            });
            await ConfirmEvents();
            _logger.LogInformation("PumpFunSendMessageEvent2:" + JsonConvert.SerializeObject(@event));
            await GrainFactory.GetGrain<IPumpFunGrain>(Guid.Parse(@event.ReplyId))
                .SendMessageAsync(@event.ReplyId, @event.ReplyMessage);
        }
    }
    
    public async Task SetPumpFunConfig(string chatId)
    {
        _logger.LogInformation("PumpFunGAgent SetPumpFunConfig, chatId:" + chatId);
        RaiseEvent(new SetPumpFunConfigEvent()
        {
            ChatId = chatId
        });
        await ConfirmEvents();
        _logger.LogInformation("PumpFunGAgent SetPumpFunConfig2, chatId:" + chatId);
    }

    public Task<PumpFunGAgentState> GetPumpFunGAgentState()
    {
        PumpFunGAgentState pumpFunGAgentState = new PumpFunGAgentState
        {
            Id = State.Id,
            ChatId = State.ChatId
        };
        return Task.FromResult(pumpFunGAgentState);
    }
}

public interface IPumpFunGAgent : IStateGAgent<PumpFunGAgentState>
{ 
    Task SetPumpFunConfig(string chatId);
    
    Task<PumpFunGAgentState> GetPumpFunGAgentState();
    
}