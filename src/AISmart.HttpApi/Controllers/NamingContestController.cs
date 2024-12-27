using System.Threading.Tasks;
using AISmart.Service;
using AISmart.Telegram;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Volo.Abp;

namespace AISmart.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("namingcontest")]
[Route("api/namingcontest/")]
public class NamingContestController: AISmartController
{
    
    private readonly ILogger<NamingContestController> _logger;
    private readonly INamingContestService _namingContestService;
    private readonly IMicroAIService _microAiService;
    
    public NamingContestController(ILogger<NamingContestController> logger, 
        INamingContestService namingContestService,IMicroAIService microAiService)
    {
        _logger = logger;
        _namingContestService = namingContestService;
        _microAiService = microAiService;
    }
    
    [HttpPost("initAgents")]
    public async Task InitAgents([FromBody]CompetitionAgentsDto competitionAgentsDto)
    {
        var headers = Request.Headers;
        var token = headers["X-Telegram-Bot-Api-Secret-Token"];
        _logger.LogInformation("Receive update message from telegram.{specificHeader}",token);
        _logger.LogInformation("Receive update message from telegram.{message}",JsonConvert.SerializeObject(updateMessage));
        await _namingContestService.InitAgentsAsync(competitionAgentsDto,token);
    }
    
    [HttpPost("initNetwork")]
    public async Task<GroupResponse> InitNetworks([FromBody]NetworksDto networksDto)
    {
        var headers = Request.Headers;
        var token = headers["X-Telegram-Bot-Api-Secret-Token"];
        _logger.LogInformation("Receive update message from telegram.{specificHeader}",token);
        _logger.LogInformation("Receive update message from telegram.{message}",JsonConvert.SerializeObject(updateMessage));
        var groupResponse = await _namingContestService.InitNetworksAsync(networksDto,token);
        return groupResponse;
    }
    
    [HttpPost("start")]
    public async Task StartGroup([FromBody]TelegramUpdateDto updateMessage)
    {
        var headers = Request.Headers;
        var token = headers["X-Telegram-Bot-Api-Secret-Token"];
        _logger.LogInformation("Receive update message from telegram.{specificHeader}",token);
        _logger.LogInformation("Receive update message from telegram.{message}",JsonConvert.SerializeObject(updateMessage));
        await _namingContestService.StartGroupAsync(updateMessage,token);
    }
}