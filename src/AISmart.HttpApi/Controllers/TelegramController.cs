using System.IO;
using System.Threading.Tasks;
using AISmart.Authors;
using AISmart.Dapr;
using AISmart.Dto;
using AISmart.Provider;
using AISmart.Service;
using AISmart.Telegram;
using Asp.Versioning;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Volo.Abp;

namespace AISmart.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("telegram")]
public class TelegramController: AISmartController
{
    private readonly ILogger<TelegramController> _logger;
    private readonly ITelegramService _telegramService;
    
    public TelegramController(ILogger<TelegramController> logger, 
        ITelegramService telegramService)
    {
        _logger = logger;
        _telegramService = telegramService;
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
}