using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.CQRS.Dto;
using AISmart.CQRS.Provider;

namespace AISmart.Service;

public class CqrsService : ICqrsService
{
    private readonly ICQRSProvider _cqrsProvider;
    
    public CqrsService(ICQRSProvider cqrsProvider)
    {
        _cqrsProvider = cqrsProvider;
    }
    
    public async Task PublishAsync(StateBase state, string id)
    {
        await _cqrsProvider.PublishAsync(state, id);
    }

    public async Task<BaseStateIndex> QueryAsync(string index, string id)
    {
        return await _cqrsProvider.QueryAsync(index, id);
    }

    public async Task SendEventCommandAsync(EventBase eventBase)
    {
        await _cqrsProvider.SendEventCommandAsync(eventBase);
    }
}