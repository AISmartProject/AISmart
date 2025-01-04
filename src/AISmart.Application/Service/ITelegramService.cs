using System;
using System.Threading.Tasks;
using AISmart.GAgent.Telegram.Dtos;
using AiSmart.GAgent.TestAgent.LoadTestAgent;
using Microsoft.Extensions.Primitives;

namespace AISmart.Service;

public interface ITelegramService
{
    public Task ReceiveMessagesAsync(TelegramUpdateDto updateMessage, StringValues token);
    
    public Task SetGroupsAsync();
    Task RegisterBotAsync(RegisterTelegramDto registerTelegramDto);
    Task SetNamingGroupAsync(string groupName);
    Task UnRegisterBotAsync(UnRegisterTelegramDto unRegisterTelegramDto);
    Task<LoadTestMessageCountResult> GetLoadTestMessageCount(string groupName);
}