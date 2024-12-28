using System.Threading.Tasks;
using AISmart.Twitter;

namespace AISmart.Service;

public interface ITwitterService
{
    Task BindTwitterAccountAsync(BindTwitterAccountDto bindTwitterAccountDto);
    Task UnbindTwitterAccountAsync(UnbindTwitterAccountDto bindTwitterAccountDto);
    Task PostTweetAsync(PostTweetDto postTweetDto);
    Task ReplyMentionAsync(ReplyMentionDto postTweetDto);
}