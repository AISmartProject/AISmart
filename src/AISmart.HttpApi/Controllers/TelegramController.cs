using System.IO;
using System.Threading.Tasks;
using AISmart.Authors;
using AISmart.Dapr;
using AISmart.Dto;
using AISmart.GAgent.Telegram.Dtos;
using AiSmart.GAgent.TestAgent.LoadTestAgent;
using AISmart.Provider;
using AISmart.Service;
using Asp.Versioning;
using Dapr;
using Google.Type;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Volo.Abp;
using DateTime = System.DateTime;

namespace AISmart.Controllers;

[Area("app")]
[ControllerName("telegram")]
[Route("api/telegram")]
public class TelegramController: AISmartController
{
    private readonly ILogger<TelegramController> _logger;
    private readonly ITelegramService _telegramService;
    private readonly IMicroAIService _microAiService;
    
    public TelegramController(ILogger<TelegramController> logger, 
        ITelegramService telegramService,IMicroAIService microAiService)
    {
        _logger = logger;
        _telegramService = telegramService;
        _microAiService = microAiService;
    }
    [HttpPost("messages")]
    public async Task PostMessages([FromBody]TelegramUpdateDto updateMessage)
    {
        var headers = Request.Headers;
        var token = headers["X-Telegram-Bot-Api-Secret-Token"];
        _logger.LogInformation("Receive update message from telegram.{specificHeader}",token);
        _logger.LogInformation("Receive update message from telegram.{message}",JsonConvert.SerializeObject(updateMessage));
        await _telegramService.ReceiveMessagesAsync(updateMessage,token);
    }
    
    [HttpPost("setGroup")]
    public async Task SetGroupsAsync()
    {
        await _telegramService.SetGroupsAsync();
    }
    [HttpPost("setVoteGroup")]
    public async Task SetVoteGroupAsync()
    {
        await _microAiService.SetGroupsAsync();
    }
    
    [HttpGet("messagesTest")]
    public async Task TestMessages(string message,string groupName)
    {
        await _microAiService.ReceiveMessagesAsync(message,groupName);
    }
    
    [HttpGet("LoadTestMessageCount")]
    public async Task<LoadTestMessageCountResult> GetLoadTestMessageCount(string groupName)
    {
        return await _telegramService.GetLoadTestMessageCount(groupName);
    }
    
    [HttpPost("registerBot")]
    public async Task RegisterBotAsync([FromBody] RegisterTelegramDto registerTelegramDto)
    {
        await _telegramService.RegisterBotAsync(registerTelegramDto);
    }
    
    [HttpPost("setnaminggroup")]
    public async Task SetNamingGroup(string groupName)
    {
        await _telegramService.SetNamingGroupAsync(groupName);
    }
    
    [HttpPost("unregisterBot")]
    public async Task UnRegisterBotAsync([FromBody] UnRegisterTelegramDto unRegisterTelegramDto)
    {
        await _telegramService.UnRegisterBotAsync(unRegisterTelegramDto);
    }
}