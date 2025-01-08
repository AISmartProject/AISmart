using System.Threading;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.CQRS.Dto;
using MediatR;
using Nest;

namespace AISmart.CQRS.Handler;

public class GetStateQueryHandler : IRequestHandler<GetStateQuery, BaseStateIndex>
{
    private readonly IIndexingService  _indexingService ;

    public GetStateQueryHandler(
        IIndexingService indexingService
    )
    {
        _indexingService = indexingService;

    }
    
    public async Task<BaseStateIndex> Handle(GetStateQuery request, CancellationToken cancellationToken)
    {
        return await _indexingService.QueryStateIndexAsync(request.Id, request.Index);
    }
}