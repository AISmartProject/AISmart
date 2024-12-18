using System.Threading.Tasks;
using AISmart.Dto;
using Microsoft.AspNetCore.Mvc;

namespace AISmart.Provider;

public interface IChatProvider
{
    public Task<ResponseDto> ReceiveMessagesAsync(MessageDto message);
    public Task SendMessageAsync(SendMessageDto message);
}