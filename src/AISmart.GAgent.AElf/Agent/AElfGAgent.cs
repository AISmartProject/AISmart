using System;
using System.Threading;
using System.Threading.Tasks;
using AISmart.Agent.Event;
using AISmart.Agent.Events;
using AISmart.Agent.GEvents;
using AISmart.Agent.Grains;
using AISmart.Agents;
using AISmart.Application.Grains;
using AISmart.Dapr;
using AISmart.Dto;
using AISmart.GAgent.Core;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Orleans.Streams;

namespace AISmart.Agent;

[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class AElfGAgent : GAgentBase<AElfAgentGState, TransactionGEvent>, IAElfAgent
{
    public AElfGAgent(ILogger<AElfGAgent> logger) : base(logger)
    {
    }
    
    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("An agent to inform other agents when a aelf thread is published.");
    }

    [EventHandler]
    protected async Task ExecuteAsync(CreateTransactionEvent gEventData)
    {
       var gEvent = new CreateTransactionGEvent
        {
            ChainId = gEventData.ChainId,
            SenderName = gEventData.SenderName,
            ContractAddress = gEventData.ContractAddress,
            MethodName = gEventData.MethodName,
        };
        RaiseEvent(gEvent);
        await ConfirmEvents();
        _= GrainFactory.GetGrain<ITransactionGrain>(gEvent.Id).SendAElfTransactionAsync(
            new SendTransactionDto
            {
                Id = gEvent.Id,
                ChainId = gEventData.ChainId,
                SenderName = gEventData.SenderName,
                ContractAddress = gEventData.ContractAddress,
                MethodName = gEventData.MethodName,
                Param = gEventData.Param
            });
        Logger.LogInformation("ExecuteAsync: AElf {MethodName}", gEventData.MethodName);
    }
    
    [EventHandler]
    public Task ExecuteAsync(SendTransactionCallBackEvent gEventData)
    {
        RaiseEvent(new SendTransactionGEvent
        {
            CreateTransactionGEventId = gEventData.CreateTransactionGEventId,
            ChainId = gEventData.ChainId,
            TransactionId = gEventData.TransactionId
        });
       
        _= GrainFactory.GetGrain<ITransactionGrain>(gEventData.Id).LoadAElfTransactionResultAsync(
            new QueryTransactionDto
            {
                CreateTransactionGEventId = gEventData.CreateTransactionGEventId,
                ChainId = gEventData.ChainId,
                TransactionId = gEventData.TransactionId
            });
        return Task.CompletedTask;
    }

    [EventHandler]
    public async Task ExecuteAsync(QueryTransactionCallBackEvent gEventData)
    {
        if (gEventData.IsSuccess)
        {
            RaiseEvent(new TransactionSuccessGEvent
            {
                CreateTransactionGEventId = gEventData.CreateTransactionGEventId
            });
        }
        else
        {
            RaiseEvent(new TransactionFailedGEvent()
            {
                CreateTransactionGEventId = gEventData.CreateTransactionGEventId,
                Error = gEventData.Error
            });
        }
        await ConfirmEvents();
    }

    public async Task ExecuteTransactionAsync(CreateTransactionEvent gEventData)
    {
        await ExecuteAsync( gEventData);
    }

    public async Task<AElfAgentGState> GetAElfAgentDto()
    {
        AElfAgentDto aelfAgentDto = new AElfAgentDto();
        aelfAgentDto.Id = State.Id;
        aelfAgentDto.PendingTransactions = State.PendingTransactions;
        return aelfAgentDto;
    }
    

    
    protected Task ExecuteAsync(TransactionGEvent eventData)
    {
        return Task.CompletedTask;
    }
}

public interface IAElfAgent : IGrainWithGuidKey
{ 
    Task ExecuteTransactionAsync(CreateTransactionEvent gEventData);
    Task<AElfAgentGState> GetAElfAgentDto();
}

