using System.Threading.Tasks;
using AISmart.Dto;
using AISmart.PumpFun;

namespace AISmart.Service;

public interface IPumpFunChatService
{
    public Task ReceiveMessagesAsync(PumpFunInputDto inputDto);
    
    public Task<string> SetGroupsAsync(string chatId);

    public Task<PumFunResponseDto> SearchAnswerAsync(string replyId);
}