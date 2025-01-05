using System.Threading.Tasks;
using AISmart.Service;
using AISmart.Twitter;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AISmart.Controllers;

[Area("app")]
[ControllerName("twitter")]
[Route("api/twitter")]
public class TwitterController
{
    private readonly ILogger<TwitterController> _logger;
    private readonly ITwitterService  _twitterService;
    
    public TwitterController(ILogger<TwitterController> logger, ITwitterService twitterService)
    {
        _logger = logger;
        _twitterService = twitterService;
    }
    
    [Authorize]
    [HttpPost("bindAccount")]
    public async Task BindAccountAsync([FromBody] BindTwitterAccountDto bindTwitterAccountDto)
    {
        await _twitterService.BindTwitterAccountAsync(bindTwitterAccountDto);
    }
    
    [Authorize]
    [HttpPost("unbindAccount")]
    public async Task UnbindAccountAsync([FromBody] UnbindTwitterAccountDto unbindTwitterAccountDto)
    {
        await _twitterService.UnbindTwitterAccountAsync(unbindTwitterAccountDto);
    }
    
    [Authorize]
    [HttpPost("tweet")]
    public async Task PostTweetAsync([FromBody] PostTweetDto postTweetDto)
    {
        await _twitterService.PostTweetAsync(postTweetDto);
    }
    
    [Authorize]
    [HttpPost("replyMention")]
    public async Task ReplyMentionAsync([FromBody] ReplyMentionDto replyMentionDto)
    {
        await _twitterService.ReplyMentionAsync(replyMentionDto);
    }
}