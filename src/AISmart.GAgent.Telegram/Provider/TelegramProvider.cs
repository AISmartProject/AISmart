using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AISmart.Dto;
using AISmart.Helper;
using AISmart.Options;
using AISmart.Telegram;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace AISmart.Provider;

public class TelegramProvider : ITelegramProvider,ISingletonDependency
{
    private readonly ILogger<TelegramProvider> _logger;
    private readonly IOptionsMonitor<TelegramOptions> _telegramOptions;

    public TelegramProvider(ILogger<TelegramProvider> logger,IOptionsMonitor<TelegramOptions> telegramOptions)
    {
        _logger = logger;
        _telegramOptions = telegramOptions;
    }
    
    public async Task SendMessageAsync(string sendUser,string chatId, string message,ReplyParamDto? replyParam = null)
    {
        String Token = GetAccount(sendUser);
        string url = $"https://api.telegram.org/bot{Token}/sendMessage";
        
        // Create a request object
        var sendMessageRequest = new MessageParamsRequest()
        {
            ChatId = chatId,
            Text = message
        };

        // Add reply_to_message_id if present
        if (replyParam!= null)
        {
          var replyParameters = new ReplyParameters
            {
                MessageId = replyParam.MessageId.ToString()
            };
            sendMessageRequest.ReplyParameters = replyParameters;
        }

        // Serialize the request object to JSON
        string json = JsonConvert.SerializeObject(sendMessageRequest, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });

        try
        {
            _logger.LogDebug("send message to {chatId} : {message}",chatId, message);
            var response = await new HttpClient().PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(responseBody);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError($"request error: {e.Message}");
        }
    }
    
    public async Task<string> GetUpdatesAsync(string sendUser)
    {
        String Token = GetAccount(sendUser);

        string url = $"https://api.telegram.org/bot{Token}/GetUpdates";
        try
        {
            // var telUpdate = await GetAsync<TelegramUpdateDto>(url);
            HttpResponseMessage response = await new HttpClient().GetAsync(url);
                
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(responseBody);
            var res = await response.Content.DeserializeSnakeCaseAsync<GetUpdatedDto>();
            _logger.LogInformation("GetUpdatesAsync:{message}",JsonConvert.SerializeObject(res));
            return responseBody;
        }
        catch (HttpRequestException e)
        {
            _logger.LogError($"request error: {e.Message}");
        }

        return null;
    }
    
    
    public async Task SetWebhookAsync(string sendUser,string webhook, string secretToken)
    {
        String Token = GetAccount(sendUser);
        string url = $"https://api.telegram.org/bot{Token}/setWebhook";

        var parameters = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("url", webhook),
            new KeyValuePair<string, string>("secret_token", secretToken)
        });

        try
        {
            HttpResponseMessage response = await new HttpClient().PostAsync(url, parameters);
                
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(responseBody);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError($"request error: {e.Message}");
        }
    }
    
    private string GetAccount(string accountName)
    {
        var optionExists = _telegramOptions.CurrentValue.AccountDictionary.TryGetValue(accountName, out var account);
        if (!optionExists)
        {
            throw new UserFriendlyException($"Telegram Account {accountName} not found");
        }
        return account;
    }

    public async Task SendPhotoAsync(string sendUser,PhotoParamsRequest photoParamsRequest)
    {
        var token = GetAccount(sendUser);
        var url = $"https://api.telegram.org/bot{token}/sendPhoto";
        var paramsJson = JsonConvert.SerializeObject(photoParamsRequest, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
        var content = new StringContent(paramsJson, Encoding.UTF8, "application/json");
        try
        {
            var response = await new HttpClient().PostAsync(url, content);
                
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(responseBody);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError($"request error: {e.Message}");
        }
    }

    public async Task ReceiveMessagesAsync(TelegramUpdateDto updateMessage)
    {
        // To filter only messages that mention the bot, check if message.Entities.type == "mention".
        // Group message auto-reply, just add the bot as a group admin.
        if (updateMessage.Message != null)
        {
            await SendMessageAsync("Test",updateMessage.Message.Chat.Id.ToString(), "hello test message",new ReplyParamDto
            {
                MessageId = updateMessage.Message.MessageId
            });
        }
    }
}