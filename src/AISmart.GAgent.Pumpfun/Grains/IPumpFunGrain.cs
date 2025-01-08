using System.Threading.Tasks;
using Orleans;

namespace AISmart.Grains;

public interface IPumpFunGrain : IGrainWithGuidKey
{
    public Task SendMessageAsync(string replyId, string? replyMessage);
   
}