using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.CQRS.Dto;
using AISmart.GAgent.Core;

namespace AISmart.CQRS.Provider;

public interface ICQRSProvider : IEventDispatcher
{
    Task<BaseStateIndex> QueryAsync(string index, string id);
    
    Task SendEventCommandAsync(EventBase eventBase);

    Task<T> QueryGEventAsync<T>(string index, string id) where T : BaseEventIndex;


}