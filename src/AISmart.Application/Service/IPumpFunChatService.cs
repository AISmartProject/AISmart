using System.Threading.Tasks;
using AISmart.Dto;
using AISmart.PumpFun;
using AISmart.Telegram;

namespace AISmart.Service;

public interface IPumpFunChatService
{
    public Task ReceiveMessagesAsync(PumpFunInputDto inputDto);

    public Task<string> SetGroupsAsync(string chatId, string bio);

    public Task<PumFunResponseDto> SearchAnswerAsync(string replyId);
}