using System.Threading.Tasks;
using AISmart.Authors;
using AISmart.Dapr;
using AISmart.Rag;
using AISmart.Rag.dto;
using Asp.Versioning;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Volo.Abp;

namespace AISmart.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("rag")]
public class RagController
{
    private readonly ILogger<RagController> _logger;
    private readonly IRagProvider _ragProvider;
    
    public RagController(ILogger<RagController> logger)
    {
        _logger = logger;
        _ragProvider = new RagProvider();
    }
    
    [HttpPost("add")]
    public Task Add( AddTextInput input)
    {
        _logger.LogInformation("add {text}", input.Text);
        return _ragProvider.StoreTextAsync(input.Text);
    }
    
    [HttpGet("retrieve")]
    public Task<string> Retrieve( RetrieveInput input)
    {
        _logger.LogInformation("retrieve {text}", input.Query);
        return _ragProvider.RetrieveAnswerAsync(input.Query);
    }
    
}