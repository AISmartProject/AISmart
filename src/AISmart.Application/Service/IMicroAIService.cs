using System.Threading.Tasks;

namespace AISmart.Service;

public interface IMicroAIService
{
    public Task ReceiveMessagesAsync(string message,string groupName);
    
    public Task SetGroupsAsync();
}