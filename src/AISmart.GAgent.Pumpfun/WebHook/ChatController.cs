using System.Threading.Tasks;
using AISmart.Dto;
using AISmart.Provider;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Volo.Abp;

namespace AISmart.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("chat")]
public class ChatController
{
    private readonly ILogger<ChatController> _logger;
    private readonly IChatProvider _chatProvider;
    
    public ChatController(ILogger<ChatController> logger, IChatProvider chatProvider)
    {
        _logger = logger;
        _chatProvider = chatProvider;
    }
    
    [HttpPost("messages")]
    public async Task<ResponseDto> ReceiveMessages([FromBody]MessageDto message)
    {
        _logger.LogDebug("Receive message from chat.{message}",JsonConvert.SerializeObject(message));
        return await _chatProvider.ReceiveMessagesAsync(message);
    }
}