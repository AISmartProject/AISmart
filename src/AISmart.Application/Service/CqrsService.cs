using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.CQRS.Dto;
using AISmart.CQRS.Provider;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Volo.Abp.Application.Services;
using Volo.Abp.ObjectMapping;

namespace AISmart.Service;

public class CqrsService : ApplicationService,ICqrsService
{
    private readonly ICQRSProvider _cqrsProvider;
    private readonly IObjectMapper _objectMapper;

    public CqrsService(ICQRSProvider cqrsProvider,IObjectMapper objectMapper)
    {
        _cqrsProvider = cqrsProvider;
        _objectMapper = objectMapper;

    }
    
    public async Task<BaseStateIndex> QueryAsync(string index, string id)
    {
        return await _cqrsProvider.QueryAsync(index, id);
    }

    public async Task SendEventCommandAsync(EventBase eventBase)
    {
        await _cqrsProvider.SendEventCommandAsync(eventBase);
    }

    public async Task<K> QueryGEventAsync<T, K>(string index, string id) where T : GEventBase
    {
        var documentContent =  await _cqrsProvider.QueryGEventAsync(index, id);
        var gEvent =  JsonConvert.DeserializeObject<T>(documentContent);
        return _objectMapper.Map<T , K>(gEvent);
    }
}