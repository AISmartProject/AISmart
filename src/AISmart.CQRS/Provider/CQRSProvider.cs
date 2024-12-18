using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.CQRS.Dto;
using MediatR;
using Volo.Abp.DependencyInjection;

namespace AISmart.CQRS.Provider;

public class CQRSProvider : ICQRSProvider, ISingletonDependency
{
    private readonly IMediator _mediator;
    public CQRSProvider(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task PublishAsync(StateBase state, string id)
    {
        var command = new SaveStateCommand
        {
            Id = id,
            State = state
        };
        await _mediator.Send(command);
    }

    public async Task<BaseStateIndex> QueryAsync(string index, string id)
    {
        var getStateQuery = new GetStateQuery()
        {
            Index = index,
            Id = id
        };
        
        var state = await _mediator.Send(getStateQuery);
        return state;
    }
}