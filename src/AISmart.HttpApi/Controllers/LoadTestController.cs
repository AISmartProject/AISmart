using System.Threading.Tasks;
using AISmart.Application;
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

    public LoadTestController(ILogger<LoadTestController> logger,
        IDemoAppService demoAppService)
    {
        _logger = logger;
        _demoAppService = demoAppService;
    }

    [HttpGet("load-test")]
    public async Task PostMessages(int mockAGAgentCount, int mockBGAgentCount, int mockCGAgentCount)
    {
        await _demoAppService.AgentLoadTest(mockAGAgentCount, mockBGAgentCount, mockCGAgentCount);
    }
}