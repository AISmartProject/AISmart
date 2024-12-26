using System;
using System.Threading;
using System.Threading.Tasks;
using AISmart.CQRS.Dto;
using MediatR;
using Newtonsoft.Json;


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
    }
}