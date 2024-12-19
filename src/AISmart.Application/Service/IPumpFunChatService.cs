using System.Threading.Tasks;
using AISmart.Dto;
using AISmart.PumpFun;
using AISmart.Telegram;

namespace AISmart.Service;

public interface IPumpFunChatService
{
    public Task ReceiveMessagesAsync(PumpFunInputDto inputDto);
    
    public Task SetGroupsAsync();

    public Task<PumFunResponseDto> SearchAnswerAsync(string replyId);
}