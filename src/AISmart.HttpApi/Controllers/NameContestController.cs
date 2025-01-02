using System.Threading.Tasks;
using AISmart.Service;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Volo.Abp;

namespace AISmart.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("namingcontest")]
[Host("api/namecontest/")]
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
    
    [HttpPost]
    [Host("initagents")]
    public async Task<AgentResponse> InitAgents([FromBody]ContestAgentsDto contestAgentsDto)
    {
        var headers = Request.Headers;
        _logger.LogInformation("Receive update message  .{message}",JsonConvert.SerializeObject(contestAgentsDto));
        return await _namingContestService.InitAgentsAsync(contestAgentsDto);
    }
    
    [HttpPost]
    [Route("initnetwork")]
    public async Task<GroupResponse> InitNetworks([FromBody]NetworksDto networksDto)
    {
        var headers = Request.Headers;
        _logger.LogInformation("Receive update message from pumpfun.{message}",JsonConvert.SerializeObject(networksDto));
        var groupResponse = await _namingContestService.InitNetworksAsync(networksDto);
        return groupResponse;
    }
    
    [HttpPost]
    [Route("start")]
    public async Task StartGroup([FromBody]GroupDto groupDto)
    {
        var headers = Request.Headers;
        _logger.LogInformation("Receive update message from telegram.{message}",JsonConvert.SerializeObject(groupDto));
        await _namingContestService.StartGroupAsync(groupDto);
    }
}