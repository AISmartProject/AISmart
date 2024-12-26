using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AISmart.Dto;
using AISmart.Options;
using AISmart.PumpFun;
using AISmart.Telegram;
using AISmart.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace AISmart.Provider;

public class PumpFunProvider : IPumpFunProvider,ISingletonDependency
{
    private readonly ILogger<PumpFunProvider> _logger;
    private readonly IOptionsMonitor<PumpfunOptions> _pumpfunOptions;
    private readonly string _callBackUrl;
    private readonly string _accessToke;



    public PumpFunProvider(ILogger<PumpFunProvider> logger, IOptionsMonitor<PumpfunOptions> pumpfunOptions)
    {
        _logger = logger;
        _callBackUrl = pumpfunOptions.CurrentValue.CallBackUrl;
        _accessToke = pumpfunOptions.CurrentValue.AccessToken;
    }
    
    public async Task SendMessageAsync(string replyId, string replyMessage)
    {
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
            _logger.LogDebug("PumpFunProvider send message to {replyId} : {replyMessage}",replyId, replyMessage);
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_callBackUrl);
            client.DefaultRequestHeaders.Add("Authorization", _accessToke);
            var response = await client.PostAsync(_callBackUrl, new StringContent(json, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            _logger.LogDebug("PumpFunProvider send message2 to {replyId} : {response} : {replyMessage}",replyId, response, replyMessage);
            string responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(responseBody);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError($"request error: {e.Message}");
        }
    }
}