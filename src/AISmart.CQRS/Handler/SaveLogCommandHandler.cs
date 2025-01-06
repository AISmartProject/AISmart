using System.Threading;
using System.Threading.Tasks;
using AISmart.CQRS.Dto;
using Volo.Abp.ObjectMapping;
using MediatR;

namespace AISmart.CQRS.Handler;

public class SaveLogCommandHandler : IRequestHandler<SaveLogCommand>
{
    private readonly IIndexingService  _indexingService ;
    private readonly IObjectMapper _objectMapper;

    public SaveLogCommandHandler(
        IIndexingService indexingService,
        IObjectMapper objectMapper
    )
    {
        _indexingService = indexingService;
        _objectMapper = objectMapper;
    }

    public async Task<Unit> Handle(SaveLogCommand command, CancellationToken cancellationToken)
    {
        _indexingService.CheckExistOrCreateIndex<AIChatLogIndex>();
        await SaveIndexAsync(command);
        return Unit.Value;
    }

    private async Task SaveIndexAsync(SaveLogCommand command)
    {
        var index = _objectMapper.Map<SaveLogCommand, AIChatLogIndex>(command);
        await _indexingService.SaveOrUpdateChatLogIndexAsync(index);
    }
}