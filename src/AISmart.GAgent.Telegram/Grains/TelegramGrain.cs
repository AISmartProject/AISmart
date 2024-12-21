using System.Threading.Tasks;
using AISmart.Dto;
using AISmart.Grains;
using AISmart.Provider;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Orleans;
using Orleans.Providers;

namespace AISmart.Agent.Grains;

[StorageProvider(ProviderName = "PubSubStore")]
public class TelegramGrain : Grain<TelegramState>, ITelegramGrain
{
    public readonly ITelegramProvider _telegramProvider;
    private ILogger<TelegramGrain> _Logger;
    public TelegramGrain(ITelegramProvider telegramProvider,ILogger<TelegramGrain> Logger) 
    {
        _telegramProvider = telegramProvider;
        _Logger = Logger;
    }


    public async Task SendMessageAsync(string sendUser, string chatId, string message, string? replyMessageId)
    {
        ReplyParamDto replyParamDto = null;
        if (!replyMessageId.IsNullOrEmpty())
        {
            replyParamDto = new ReplyParamDto()
            {
                MessageId = long.Parse(replyMessageId)
            };
        }
        await _telegramProvider.SendMessageAsync(sendUser, chatId, message, replyParamDto);
    }
}