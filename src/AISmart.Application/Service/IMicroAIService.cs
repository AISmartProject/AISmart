using System.Threading.Tasks;
using AISmart.Telegram;
using Microsoft.Extensions.Primitives;

namespace AISmart.Service;

public interface IMicroAIService
{
    public Task ReceiveMessagesAsync(string message);
    
    public Task SetGroupsAsync();
}