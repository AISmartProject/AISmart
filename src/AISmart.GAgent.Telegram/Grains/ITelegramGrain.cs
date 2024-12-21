using System.Threading.Tasks;
using Orleans;

namespace AISmart.Grains;

public interface ITelegramGrain:IGrainWithGuidKey
{
    public Task SendMessageAsync(string sendUser, string chatId, string message,string? replyMessageId);
   
}