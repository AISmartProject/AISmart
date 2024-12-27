using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Dto;
using Orleans;

namespace AISmart.Grains;

public interface ITwitterGrain : IGrainWithStringKey
{
    public Task CreateTweetAsync(string text, string token, string tokenSecret);
    public Task ReplyTweetAsync(string text, string tweetId, string token, string tokenSecret);
    public Task<List<Tweet>> GetRecentMentionAsync();
}