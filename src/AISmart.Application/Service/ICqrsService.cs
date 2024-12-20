using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.CQRS.Dto;

namespace AISmart.Service;

public interface ICqrsService
{
    Task<BaseStateIndex> QueryAsync(string index, string id);
    
    Task SendEventCommandAsync(EventBase eventBase);
}