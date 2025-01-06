using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.CQRS.Dto;
using AISmart.GAgent.Core;

namespace AISmart.CQRS.Provider;

public interface ICQRSProvider : IEventDispatcher
{
    Task<BaseStateIndex> QueryAsync(string index, string id);
    
    Task SendEventCommandAsync(EventBase eventBase);

    Task<string> QueryGEventAsync(string index, string id);
    
    Task SendLogCommandAsync(SaveLogCommand command);
    
    Task<ChatLogPageResultDto> QueryChatLogListAsync(GetLogQuery command);
}