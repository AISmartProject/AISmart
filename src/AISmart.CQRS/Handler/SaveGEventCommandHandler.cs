using System;
using System.Threading;
using System.Threading.Tasks;
using AISmart.CQRS.Dto;
using MediatR;


namespace AISmart.CQRS.Handler;

public class SaveGEventCommandHandler : IRequestHandler<SaveGEventCommand>
{
    private readonly IIndexingService  _indexingService ;

    public SaveGEventCommandHandler(
        IIndexingService indexingService
    )
    {
        _indexingService = indexingService;
    }

    public async Task<Unit> Handle(SaveGEventCommand request, CancellationToken cancellationToken)
    {
        _indexingService.CheckExistOrCreateGEventIndex(request.GEvent);
        await SaveIndexAsync(request);
        return Unit.Value;
    }

    private async Task SaveIndexAsync(SaveGEventCommand request)
    {
        await _indexingService.SaveOrUpdateGEventIndexAsync(request.GEvent);
        
        /*var id = "e4746d55-c530-4b09-bffd-ca4da5f13f18";
        var result = await _indexingService.QueryEventIndexAsync<CreateTransactionGEventIndex>(id, "createtransactiongeventindex");
        if (result is CreateTransactionGEventIndex specificEvent)
        {
            Console.WriteLine($"ChainId: {specificEvent.ChainId}");
        }*/
    }
}