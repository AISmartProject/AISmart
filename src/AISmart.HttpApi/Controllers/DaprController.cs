using System.Threading.Tasks;
using AISmart.Authors;
using AISmart.Dapr;
using Asp.Versioning;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Volo.Abp;

namespace AISmart.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("dapr")]
public class DaprController
{
    private readonly ILogger<DaprController> _logger;
    public DaprController(ILogger<DaprController> logger)
    {
        _logger = logger;
    }
}