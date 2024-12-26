using System.Threading.Tasks;
using AISmart.Dto;
using AISmart.Options;
using AISmart.Provider;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;

namespace AISmart.Grains;

public class TwitterGrain : Grain<TwitterState>, ITwitterGrain
{
    private readonly ITwitterProvider _twitterProvider;
    private ILogger<TwitterGrain> _logger;
    private readonly IOptionsMonitor<TwitterOptions> _twitterOptions;
    
    public TwitterGrain(ITwitterProvider twitterProvider, 
        ILogger<TwitterGrain> logger, 
        IOptionsMonitor<TwitterOptions> twitterOptions) 
    {
        _twitterProvider = twitterProvider;
        _logger = logger;
        _twitterOptions = twitterOptions;
    }
    
    public async Task CreateTweetAsync(string text, string accountName)
    {
        await _twitterProvider.PostTwitterAsync(text, accountName);
    }
    
    public async Task ReplyTweetAsync(string text, string tweetId)
    {
        await _twitterProvider.ReplyAsync(text, tweetId);
    }
}