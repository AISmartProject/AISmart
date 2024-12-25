using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Events;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.Grains;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;

public class CreativeGAgent : MicroAIGAgent, ICreativeGAgent
{
    private readonly TelegramTestOptions _telegramTestOptions;

    public CreativeGAgent(IOptions<TelegramTestOptions> options, ILogger<MicroAIGAgent> logger) : base(logger)
    {
        _telegramTestOptions = options.Value;
    }

    [EventHandler]
    public async Task HandleEventAsync(TrafficInformCreativeGEvent @event)
    {
        if (@event.CreativeGrainId != this.GetPrimaryKey())
        {
            return;
        }

        var message = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
            .SendAsync(@event.NamingContent, new List<MicroAIMessage>());
        if (message != null && !message.Content.IsNullOrEmpty())
        {
            var namingReply = message.Content;

            await this.PublishAsync(new NamedCompleteGEvent()
            {
                Content = @event.NamingContent,
                GrainGuid = this.GetPrimaryKey(),
                NamingReply = namingReply,
                CreativeName = State.AgentName,
            });
            
            await PublishAsync(new SendMessageEvent()
            {
                ChatId = _telegramTestOptions.ChatId,
                Message = $"Creative {State.AgentName} Naming:{namingReply}"
            });
        }
    }
}