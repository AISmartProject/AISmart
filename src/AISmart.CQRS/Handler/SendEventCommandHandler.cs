using System;
using System.Threading;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.Agents.Group;
using AISmart.CQRS.Dto;
using AISmart.Sender;
using MediatR;
using Orleans;

namespace AISmart.CQRS.Handler;

public class SendEventCommandHandler : IRequestHandler<SendEventCommand>
{
    private readonly IGrainFactory _clusterClient;

    public SendEventCommandHandler(IGrainFactory clusterClient
    )
    {
        _clusterClient = clusterClient;
    }

    public async Task<Unit> Handle(SendEventCommand request, CancellationToken cancellationToken)
    {
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
        var groupAgent = _clusterClient.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());

        await publishingAgent.PublishToAsync(groupAgent);
        await publishingAgent.PublishEventAsync(request.Event);
        return Unit.Value; 
    }
    
}