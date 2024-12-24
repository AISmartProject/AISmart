using System.Threading;
using System.Threading.Tasks;
using AISmart.CQRS.Dto;
using AISmart.CQRS.Service;
using MediatR;

namespace AISmart.CQRS.Handler;

public class SaveStateCommandHandler : IRequestHandler<SaveStateCommand>
{
    private readonly KafkaProducerService _kafkaProducerService;

    public SaveStateCommandHandler(
       KafkaProducerService kafkaProducerService
    )
    {
       _kafkaProducerService = kafkaProducerService;
    }

    public async Task<Unit> Handle(SaveStateCommand request, CancellationToken cancellationToken)
    {
        await _kafkaProducerService.SendAsync(request);
        return Unit.Value;
    }
}