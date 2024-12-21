using System.Threading.Tasks;
using AISmart.Rag;
using AISmart.Rag.Dto;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace AISmart.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("rag")]
public class RagController
{
    private readonly IRagProvider _ragProvider;
    
    public RagController(IRagProvider ragProvider)
    {
        _ragProvider = ragProvider;
    }
    
    [HttpPost("add")]
    public Task Add( AddTextInput input)
    {
        return _ragProvider.StoreTextAsync(input.Text);
    }
    
    [HttpGet("retrieve")]
    public Task<string> Retrieve( RetrieveInput input)
    {
        return _ragProvider.RetrieveAnswerAsync(input.Query);
    }
    
}