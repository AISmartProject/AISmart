using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AISmart.Dto;
using AISmart.PumpFun;
using AISmart.Telegram;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace AISmart.Provider;

public class PumpFunProvider : IPumpFunProvider,ISingletonDependency
{
    private readonly ILogger<PumpFunProvider> _logger;

    public PumpFunProvider(ILogger<PumpFunProvider> logger)
    {
        _logger = logger;
    }
    
    // TODO: it seems that this method is not needed?
    // public async Task ReceiveMessagesAsync(ChatInputDto message)
    // {
    //     // To filter only messages that mention the bot, check if message.Entities.type == "mention".
    //     // Group message auto-reply, just add the bot as a group admin.
    //     if (message.Message != null)
    //     {
    //         await SendMessageAsync("Test",updateMessage.Message.Chat.Id.ToString(), "hello test message",new ReplyParamDto
    //         {
    //             MessageId = updateMessage.Message.MessageId
    //         });
    //     }
    // }
    
    public async Task SendMessageAsync(string replyId, string replyMessage)
    {
        const string url = $"https://jump-fun-testnet.aelf.dev/api/app/chat/agent-callback";
        
        // TODO:can use this model? chatId?requestMessageId?
        var sendMessageRequest = new PumFunResponseDto()
        {
            // TODO:
            ReplyId = replyId,
            ReplyMessage = replyMessage
        };

        // Serialize the request object to JSON, TODO:回调需要加TOKEN
        var json = JsonConvert.SerializeObject(sendMessageRequest, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });

        try
        {
            _logger.LogDebug("send message to {replyId} : {replyMessage}",replyId, replyMessage);
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
    
    // TODO:needed?
    // public async Task SetWebhookAsync(string sendUser,string webhook, string secretToken)
    // {
    //     String Token = GetAccount(sendUser);
    //     string url = $"https://api.telegram.org/bot{Token}/setWebhook";
    //
    //     var parameters = new FormUrlEncodedContent(new[]
    //     {
    //         new KeyValuePair<string, string>("url", webhook),
    //         new KeyValuePair<string, string>("secret_token", secretToken)
    //     });
    //
    //     try
    //     {
    //         HttpResponseMessage response = await new HttpClient().PostAsync(url, parameters);
    //             
    //         response.EnsureSuccessStatusCode();
    //
    //         string responseBody = await response.Content.ReadAsStringAsync();
    //         _logger.LogInformation(responseBody);
    //     }
    //     catch (HttpRequestException e)
    //     {
    //         _logger.LogError($"request error: {e.Message}");
    //     }
    // }
    
    // TODO:needed?
    // private string GetAccount(string accountName)
    // {
    //     var optionExists = _telegramOptions.CurrentValue.AccountDictionary.TryGetValue(accountName, out var account);
    //     if (!optionExists)
    //     {
    //         throw new UserFriendlyException($"Telegram Account {accountName} not found");
    //     }
    //     return account;
    // }
    
}