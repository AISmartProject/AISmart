using System.Threading.Tasks;
using AElf.Client.Dto;
using AElf.Contracts.MultiToken;
using AElf.Types;
using AISmart.Provider;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AISmart.GAgent;

public class TwitterGAgentTest : AISmartApplicationTestBase
{
    private readonly ITwitterProvider _twitterProvider;
    private readonly ITestOutputHelper _output;
    public TwitterGAgentTest(ITestOutputHelper output)
    {
        _twitterProvider = GetRequiredService<ITwitterProvider>();
        _output = output;
    }
    
    [Fact]
    public async Task PostTwittersAsyncTest ()
    {
        var accessToken = "";
        var accessTokenSecret = "";
        await _twitterProvider.PostTwitterAsync( "Today is Friday！", accessToken, accessTokenSecret);
    }
    
    [Fact]
    public async Task ReplyAsyncAsyncTest ()
    {
        var tweetId = "1873625128661381262";
        var accessToken = "";
        var accessTokenSecret = "";
        await _twitterProvider.ReplyAsync("Today is Friday！", tweetId, accessToken, accessTokenSecret);
    }
    
    [Fact]
    public async Task QueryRecentTwittersAsyncTest ()
    {
        var userName = "elonMusk";
        var resp = await _twitterProvider.GetMentionsAsync(userName);
        resp.Count.ShouldNotBe(0);
    }
}