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
    
    [Fact]
    public async Task GetUserNameAsyncTest ()
    {
        var accessToken = "";
        var accessTokenSecret = "";
        var resp = await _twitterProvider.GetUserName(accessToken, accessTokenSecret);
        resp.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task LikeTweetAsyncTest ()
    {
        var accessToken = "";
        var accessTokenSecret = "";
        var userId = "773374682";
        var tweetId = "1875073080164569187";
        await _twitterProvider.LikeAsync(tweetId, userId, accessToken, accessTokenSecret);
    }
    
    [Fact]
    public async Task RetweetAsyncTest ()
    {
        var accessToken = "";
        var accessTokenSecret = "";
        var userId = "773374682";
        var tweetId = "1875088621944107473";
        await _twitterProvider.RetweetAsync(tweetId, userId, accessToken, accessTokenSecret);
    }
    
    [Fact]
    public async Task QuoteTweetAsyncTest ()
    {
        var accessToken = "773374682-WheXJ5CvSI6LIcHRAG4Q2KVrGryaVVKUQ4x4xMuK";
        var accessTokenSecret = "I16LrqCsjskj0kVSkPxRAlThmbF6FdyMCiJGQI2tohcII";
        var tweetId = "1875073080164569187";
        await _twitterProvider.QuoteTweetAsync(tweetId, "Sounds interesting!", accessToken, accessTokenSecret);
    }
}