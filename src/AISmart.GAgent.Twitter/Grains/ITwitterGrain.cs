using System.Threading.Tasks;
using Orleans;

namespace AISmart.Grains;

public interface ITwitterGrain : IGrainWithGuidKey
{
    public Task CreateTweetAsync(string text, string accountName);
    public Task ReplyTweetAsync(string text, string tweetId);
}