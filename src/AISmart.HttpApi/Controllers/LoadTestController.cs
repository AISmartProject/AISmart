using AISmart.Agent.GEvents;
using AISmart.Application;
using AISmart.Dto;
using AISmart.Service;
using System.Threading.Tasks;
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

    [HttpGet("event/query")]
    public async Task<BindTwitterAccountGEventDto> PostMessages(string index,string id)
    {
        var result =  await _cqrsService.QueryGEventAsync<BindTwitterAccountGEvent, BindTwitterAccountGEventDto>(index, id);
        return result;
    }
}