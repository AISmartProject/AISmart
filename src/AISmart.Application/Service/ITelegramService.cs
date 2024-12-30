using System.Threading.Tasks;
using AISmart.Telegram;
using Microsoft.Extensions.Primitives;

namespace AISmart.Service;

public interface ITelegramService
{
    public Task ReceiveMessagesAsync(TelegramUpdateDto updateMessage, StringValues token);
    
    public Task SetGroupsAsync();
    Task RegisterBotAsync(RegisterTelegramDto registerTelegramDto);
    Task SetNamingGroupAsync(string groupName);
    Task UnRegisterBotAsync(UnRegisterTelegramDto unRegisterTelegramDto);
}