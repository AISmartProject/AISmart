using System.Threading.Tasks;
using AElf.Client.Dto;
using AElf.Contracts.MultiToken;
using AElf.Types;
using AISmart.Provider;
using Newtonsoft.Json;
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
    public async Task GetLatestTwittersAsyncTest()
    {
        var twittersAsync = await _twitterProvider.GetLatestTwittersAsync("Test", "903565929719541760", "");
        foreach (var twitter in twittersAsync)
        {
            _output.WriteLine("twitter: Id" + twitter.Id);
            _output.WriteLine("twitter: Text" + twitter.Text);
        }
    }
    
    [Fact]
    public async Task PostTwittersAsyncTest ()
    {
        var twittersAsync = await _twitterProvider.PostTwitterAsync("Test", "twitter test");
        _output.WriteLine("twitter: Id" + twittersAsync);
    }

}