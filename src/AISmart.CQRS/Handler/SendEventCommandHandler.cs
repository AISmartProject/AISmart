using System;
using System.Threading;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.CQRS.Dto;
using AISmart.Sender;
using MediatR;
using Orleans;

namespace AISmart.CQRS.Handler;

public class SendEventCommandHandler : IRequestHandler<SendEventCommand>
{
    private readonly IClusterClient _clusterClient;

    public SendEventCommandHandler(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    public async Task<Unit> Handle(SendEventCommand request, CancellationToken cancellationToken)
    {
        var publishingAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
        var groupAgent = _clusterClient.GetGrain<IGAgent>(Guid.NewGuid());

        await publishingAgent.PublishTo(groupAgent);
        await publishingAgent.PublishEventAsync(request.Event);
        return Unit.Value; 
    }
    
}