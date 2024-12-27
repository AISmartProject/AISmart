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
[Route("api/namecontest/")]
public class NameContestController: AISmartController
{
    
    private readonly ILogger<NameContestController> _logger;
    private readonly INamingContestService _namingContestService;
    private readonly IMicroAIService _microAiService;
    
    public NameContestController(ILogger<NameContestController> logger, 
        INamingContestService namingContestService,IMicroAIService microAiService)
    {
        _logger = logger;
        _namingContestService = namingContestService;
        _microAiService = microAiService;
    }
    
    [HttpPost("initagents")]
    public async Task InitAgents([FromBody]ContestAgentsDto contestAgentsDto)
    {
        var headers = Request.Headers;
        var token = headers["X-Telegram-Bot-Api-Secret-Token"];
        _logger.LogInformation("Receive update message from telegram.{specificHeader}",token);
        _logger.LogInformation("Receive update message from telegram.{message}",JsonConvert.SerializeObject(updateMessage));
        await _namingContestService.InitAgentsAsync(contestAgentsDto,token);
    }
    
    [HttpPost("initnetwork")]
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
    public async Task StartGroup([FromBody]GroupDto groupDto)
    {
        var headers = Request.Headers;
        var token = headers["X-Telegram-Bot-Api-Secret-Token"];
        _logger.LogInformation("Receive update message from telegram.{specificHeader}",token);
        _logger.LogInformation("Receive update message from telegram.{message}",JsonConvert.SerializeObject(updateMessage));
        await _namingContestService.StartGroupAsync(groupDto,token);
    }
}