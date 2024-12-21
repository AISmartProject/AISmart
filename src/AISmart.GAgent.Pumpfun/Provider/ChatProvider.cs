using System.Threading.Tasks;
using AISmart.Dto;

namespace AISmart.Provider;

public class ChatProvider : IChatProvider
{
    public Task<ResponseDto> ReceiveMessagesAsync(MessageDto message)
    {
        //todo: process message
        throw new System.NotImplementedException();
    }

    public Task SendMessageAsync(SendMessageDto message)
    {
        //todo: get message and callback to pumpfun
        throw new System.NotImplementedException();
    }
}