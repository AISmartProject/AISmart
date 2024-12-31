using System;
using System.Threading.Tasks;
using AElf.Client.Dto;
using AElf.Contracts.MultiToken;
using AElf.Types;
using AISmart.Agent;
using AISmart.Agent.Grains;
using AISmart.Application.Grains.Agents.Publisher;
using AISmart.Dto;
using AISmart.GAgent.Telegram.Dtos;
using AISmart.GAgent.Telegram.Provider;
using AISmart.Provider;
using AISmart.Service;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace AISmart.GAgent;

public class TelegramGAgentTest : AISmartApplicationTestBase
{
    private readonly ITelegramProvider _telegramProvider;
    private readonly ITelegramService _telegramService;
    private readonly ITestOutputHelper _output;
    public TelegramGAgentTest(ITestOutputHelper output)
    {
        _telegramProvider = GetRequiredService<ITelegramProvider>();
        _telegramService =  GetRequiredService<ITelegramService>();
        _output = output;
    }
    //https://core.telegram.org/bots/api#chat
    [Fact]
    public async Task SendMessageTest()
    {
    //  var updates = await  _telegramProvider.GetUpdatesAsync("Test");
     // _output.WriteLine("updates: " + updates);
     string jsonString = @"{
       ""message_id"": 12345,
       ""date"": 1617735123,
       ""text"": ""Hello, World!""
      }";
     var json = JsonConvert.DeserializeObject<Message>(jsonString);
      _output.WriteLine("updates: " + json.MessageId);
    }
    
    [Fact]
    public async Task SendMessageWithReplayTest()
    {
        await  _telegramProvider.SendMessageAsync("Test","-1002473003637","hello funtest",new ReplyParamDto
        {
            MessageId = 10
        });
    }
    
    [Fact]
    public async Task SendPhotoWithReplayTest()
    {
        await  _telegramProvider.SendPhotoAsync("Test",new PhotoParamsRequest
        {
            ChatId = "-1002473003637",
            Photo = "https://raw.githubusercontent.com/paulazhou/picbed/main/Hexo/2021_05_23_G1OwSTxDrfVlPdv.png",
            ReplyParameters = new ReplyParameters()
            {
                MessageId = "12"
            },
            Caption = "hello, this is a photo."
        });
    }
    
}