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
        var id = "c07bc5c5-2e02-456a-81c8-e6e0e975947d";
        var result = await _indexingService.QueryEventIndexV2Async(id, "createtransactiongeventindex");
        foreach (var document in result.Documents)
        {
            var deserializedObject = JsonConvert.DeserializeObject(document,typeof(CreateTransactionGEventIndex));
            deserializedObject.ToString();
        }
    }
}