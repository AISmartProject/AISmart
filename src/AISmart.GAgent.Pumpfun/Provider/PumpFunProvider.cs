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
    
    public async Task SendMessageAsync(string replyId, string replyMessage)
    {
        const string url = $"https://jump-fun-testnet.aelf.dev/api/app/chat/agent-callback";
        
        var sendMessageRequest = new PumFunResponseDto()
        {
            ReplyId = replyId,
            ReplyMessage = replyMessage
        };

        // Serialize the request object to JSON
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
}