using System;
using System.Threading.Tasks;
using AISmart.Dto;
using AISmart.PumpFun;
using AISmart.Service;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Volo.Abp;

namespace AISmart.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("pumpfun")]
[Route("api/pumpfun")]
public class PumpFunController : AISmartController
{
    private readonly ILogger<PumpFunController> _logger;
    private readonly IPumpFunChatService _pumpFunChatService;
    
    public PumpFunController(ILogger<PumpFunController> logger, IPumpFunChatService pumpFunChatService)
    {
        _logger = logger;
        _pumpFunChatService = pumpFunChatService;
    }
    
    [Authorize]
    [HttpPost]
    [Route("setGroup")]
    public async Task<string> SetGroupsAsync(string chatId)
    {
        _logger.LogInformation("PumpFunController SetGroupsAsync, chatId:{chatId}, botName:{botName}", chatId);
        return await _pumpFunChatService.SetGroupsAsync(chatId);
    }
    
    
    [HttpPost]
    [Route("chat")]
    [Authorize]
    public Task<PumpFunOutputDto> Chat([FromBody]PumpFunInputDto inputDto)
    {
        var replyId = Guid.NewGuid().ToString();
        _logger.LogInformation("PumpFunController chat, UserIdentityName:{UserIdentityName}", User.Identity.Name);
        _logger.LogInformation("PumpFunController chat, inputDto:{inputDto}",JsonConvert.SerializeObject(inputDto));

        inputDto.ReplyId = replyId;
        _pumpFunChatService.ReceiveMessagesAsync(inputDto);

        var outputDto = new PumpFunOutputDto
        {
            ReplyId = replyId
        };
        return Task.FromResult(outputDto);
    }
    
    
    [Authorize]
    [HttpGet("search")]
    public Task<PumFunResponseDto> SearchByReplyId(string replyId)
    {
        _logger.LogInformation("PumpFunController SearchByReplyId, replyId:{replyId}", replyId);
        var response = _pumpFunChatService.SearchAnswerAsync(replyId);
        return Task.FromResult(response.Result);
    }
}