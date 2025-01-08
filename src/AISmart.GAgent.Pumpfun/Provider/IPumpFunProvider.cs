using System.Threading.Tasks;
using AISmart.Dto;
using AISmart.PumpFun;

namespace AISmart.Provider;

public interface IPumpFunProvider
{
    public Task SendMessageAsync(string replyId, string replyMessage);
}