using System.Threading;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.CQRS.Dto;
using MediatR;
using Nest;

namespace AISmart.CQRS.Handler;

public class GetStateQueryHandler : IRequestHandler<GetStateQuery, BaseStateIndex>
{
    //private readonly IElasticClient _elasticClient;
    private readonly IIndexingService  _indexingService ;

    public GetStateQueryHandler(
       // IElasticClient elasticClient,
        IIndexingService indexingService
    )
    {
        //_elasticClient = elasticClient;
        _indexingService = indexingService;

    }
    
    public async Task<BaseStateIndex> Handle(GetStateQuery request, CancellationToken cancellationToken)
    {
        return await _indexingService.QueryIndexAsync(request.Id, request.Index);
    }
}