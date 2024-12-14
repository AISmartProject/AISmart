using System;
using System.Threading;
using System.Threading.Tasks;
using AISmart.Agent.Event;
using AISmart.Agent.Events;
using AISmart.Agent.GEvents;
using AISmart.Agent.Grains;
using AISmart.Application.Grains;
using AISmart.Dto;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;

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


    protected async Task ExecuteAsync(CreateTransactionEvent gEventData)
    {
       var gEvent = new CreateTransactionGEvent
        {
            ChainId = gEventData.ChainId,
            SenderName = gEventData.SenderName,
            ContractAddress = gEventData.ContractAddress,
            MethodName = gEventData.MethodName,
        };
        base.RaiseEvent(gEvent);
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
    
    protected  Task ExecuteAsync(SendTransactionCallBackEvent gEventData)
    {
        base.RaiseEvent(new SendTransactionGEvent
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

   

    protected async Task ExecuteAsync(QueryTransactionCallBackEvent gEventData)
    {
        if (gEventData.IsSuccess)
        {
            base.RaiseEvent(new TransactionSuccessGEvent
            {
                CreateTransactionGEventId = gEventData.CreateTransactionGEventId
            });
        }
        else
        {
            base.RaiseEvent(new TransactionFailedGEvent()
            {
                CreateTransactionGEventId = gEventData.CreateTransactionGEventId,
                Error = gEventData.Error
            });
        }
        await ConfirmEvents();
    }


    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
         await base.OnActivateAsync(cancellationToken);
         await SubscribeAsync<CreateTransactionEvent>(ExecuteAsync);
         await SubscribeAsync<SendTransactionCallBackEvent>(ExecuteAsync);
         await SubscribeAsync<QueryTransactionCallBackEvent>(ExecuteAsync);
    }

    public async Task ExecuteTransactionAsync(CreateTransactionGEvent gEventData)
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
        throw new NotImplementedException();
    }
}

public interface IAElfAgent : IGrainWithGuidKey
{ 
    Task ExecuteTransactionAsync(CreateTransactionGEvent gEventData);
    Task<AElfAgentGState> GetAElfAgentDto();
}