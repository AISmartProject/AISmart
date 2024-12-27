using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.CQRS.Dto;
using AISmart.CQRS.Provider;
using Volo.Abp.Application.Services;

namespace AISmart.Service;

public class CqrsService : ApplicationService,ICqrsService
{
    private readonly ICQRSProvider _cqrsProvider;
    
    public CqrsService(ICQRSProvider cqrsProvider)
    {
        _cqrsProvider = cqrsProvider;
    }
    
    public async Task<BaseStateIndex> QueryAsync(string index, string id)
    {
        return await _cqrsProvider.QueryAsync(index, id);
    }

    public async Task SendEventCommandAsync(EventBase eventBase)
    {
        await _cqrsProvider.SendEventCommandAsync(eventBase);
    }

    public async Task<T> QueryGEventAsync<T>(string index, string id) where T : BaseEventIndex
    {
        return await _cqrsProvider.QueryGEventAsync<T>(index, id);
    }
}