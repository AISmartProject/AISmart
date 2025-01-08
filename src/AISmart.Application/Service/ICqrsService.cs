using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.CQRS.Dto;

namespace AISmart.Service;

public interface ICqrsService
{
    Task<BaseStateIndex> QueryAsync(string index, string id);
    
    Task SendEventCommandAsync(EventBase eventBase);

    Task<K> QueryGEventAsync<T,K>(string index, string id) where T : GEventBase;
    
    Task<ChatLogPageResultDto> QueryChatLogListAsync(GetLogQuery command);
}