using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AISmart.Application;
using AISmart.CQRS.Dto;
using AISmart.Service;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Volo.Abp;

namespace AISmart.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("loadTest")]
public class LoadTestController : AISmartController
{
    private readonly ILogger<LoadTestController> _logger;
    private readonly IDemoAppService _demoAppService;
    private readonly ICqrsService _cqrsService;

    public LoadTestController(ILogger<LoadTestController> logger,
        IDemoAppService demoAppService,ICqrsService cqrsService)
    {
        _logger = logger;
        _demoAppService = demoAppService;
        _cqrsService = cqrsService;

    }

    [HttpGet("load-test")]
    public async Task PostMessages(int mockAGAgentCount, int mockBGAgentCount, int mockCGAgentCount)
    {
        await _demoAppService.AgentLoadTest(mockAGAgentCount, mockBGAgentCount, mockCGAgentCount);
    }
    
    [HttpGet("event/query")]
    public async Task<CreateTransactionGEventDto> PostMessages(string index,string id)
    {
       var result =  await _cqrsService.QueryGEventAsync<CreateTransactionGEvent, CreateTransactionGEventDto>(index, id);
       return result;
    }
}