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
    
    //https://developer.twitter.com/en/portal/products
    //https://developer.twitter.com/apitools/api

    
    
    [Fact]
    public async Task QueryRecentTwittersAsyncTest ()
    {
        var userName = "elonMusk";
        var resp = await _twitterProvider.GetMentionsAsync(userName);
        resp.Count.ShouldNotBe(0);
    }
}