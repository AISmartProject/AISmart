using System.Threading.Tasks;
using AISmart.GAgent.Telegram.Dtos;
using Microsoft.Extensions.Primitives;

namespace AISmart.Service;

public interface ITelegramService
{
    public Task ReceiveMessagesAsync(TelegramUpdateDto updateMessage, StringValues token);
    
    public Task SetGroupsAsync();
    Task RegisterBotAsync(RegisterTelegramDto registerTelegramDto);
    Task SetNamingGroupAsync();
    Task UnRegisterBotAsync(UnRegisterTelegramDto unRegisterTelegramDto);
}