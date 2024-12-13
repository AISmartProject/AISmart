using System.Threading.Tasks;
using AElf.Client.Dto;
using AElf.Contracts.MultiToken;
using AElf.Types;
using AISmart.Dto;
using AISmart.Provider;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace AISmart.GAgent;

public class TelegramGAgentTest : AISmartApplicationTestBase
{
    private readonly ITelegramProvider _telegramProvider;
    private readonly ITestOutputHelper _output;
    public TelegramGAgentTest(ITestOutputHelper output)
    {
        _telegramProvider = GetRequiredService<ITelegramProvider>();
        _output = output;
    }
    //https://core.telegram.org/bots/api#chat
    [Fact]
    public async Task SendMessageTest()
    {
    //  var updates = await  _telegramProvider.GetUpdatesAsync("Test");
     // _output.WriteLine("updates: " + updates);
      await  _telegramProvider.SendMessageAsync("Test","7600783090","hello bot2");
    }
    
    [Fact]
    public async Task SendMessageWithReplayTest()
    {
        await  _telegramProvider.SendMessageAsync("Test","7027097058","hello bot2",new ReplyParamDto
        {
            MessageId = 12
        });
    }
    
    [Fact]
    public async Task SendPhotoWithReplayTest()
    {
        await  _telegramProvider.SendPhotoAsync("Test",new PhotoParamsDto
        {
            ChatId = "7027097058",
            Photo = "https://raw.githubusercontent.com/paulazhou/picbed/main/Hexo/2021_05_23_G1OwSTxDrfVlPdv.png",
            ReplyParameters = new ReplyParamDto
            {
                MessageId = 12
            },
            Caption = "hello, this is a photo."
        });
    }
}