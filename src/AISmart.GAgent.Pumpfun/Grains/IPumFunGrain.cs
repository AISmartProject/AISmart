using System.Threading.Tasks;
using Orleans;

namespace AISmart.Grains;

public interface IPumFunGrain : IGrainWithGuidKey
{
    public Task SendMessageAsync(string replyId, string? replyMessage);
   
}