using System.Threading.Tasks;
using Orleans;

namespace AISmart.Grains;

public interface ITwitterGrain : IGrainWithStringKey
{
    public Task CreateTweetAsync(string text, string token, string tokenSecret);
    // public Task ReplyTweetAsync(string text, string token, string tokenSecret);
}