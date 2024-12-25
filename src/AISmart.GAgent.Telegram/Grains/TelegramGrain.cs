using System.Threading.Tasks;
using AISmart.Dto;
using AISmart.Grains;
using AISmart.Options;
using AISmart.Provider;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Orleans;
using Orleans.Providers;

namespace AISmart.Agent.Grains;

[StorageProvider(ProviderName = "PubSubStore")]
public class TelegramGrain : Grain<TelegramState>, ITelegramGrain
{
    private readonly ITelegramProvider _telegramProvider;
    private ILogger<TelegramGrain> _logger;
    private readonly IOptionsMonitor<TelegramOptions> _telegramOptions;
    public TelegramGrain(ITelegramProvider telegramProvider,ILogger<TelegramGrain> logger,IOptionsMonitor<TelegramOptions> telegramOptions) 
    {
        _telegramProvider = telegramProvider;
        _logger = logger;
        _telegramOptions = telegramOptions;
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

    public async Task RegisterTelegramAsync(string sendUser, string token)
    {
        await _telegramProvider.SetWebhookAsync(sendUser, _telegramOptions.CurrentValue.Webhook,token);
    }
}