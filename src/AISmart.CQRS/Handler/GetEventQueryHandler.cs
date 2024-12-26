using System.Threading;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.CQRS.Dto;
using MediatR;
using Nest;

namespace AISmart.CQRS.Handler;

public class GetEventQueryHandler : IRequestHandler<GetEventQuery, BaseStateIndex>
{
    private readonly IIndexingService  _indexingService ;

    public GetEventQueryHandler(
        IIndexingService indexingService
    )
    {
        _indexingService = indexingService;

    }
    
    /*public async Task<BaseStateIndex> Handle(GetStateQuery request, CancellationToken cancellationToken)
    {
        //return await _indexingService.QueryEventIndexAsync<GEventBase>(request.Id);
    }*/

    public Task<BaseStateIndex> Handle(GetEventQuery request, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}