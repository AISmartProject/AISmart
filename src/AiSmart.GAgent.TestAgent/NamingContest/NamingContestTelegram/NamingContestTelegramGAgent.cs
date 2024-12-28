using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Events;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.Grains;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISmart.Agent.NamingContestTelegram;

public class NamingContestTelegramGAgent : MicroAIGAgent, INamingContestTelegramGAgent
{
    private readonly TelegramTestOptions _telegramTestOptions;

    public NamingContestTelegramGAgent(IOptions<TelegramTestOptions> options, ILogger<MicroAIGAgent> logger) :
        base(logger)
    {
        _telegramTestOptions = options.Value;
    }

    [EventHandler]
    public async Task HandleEventAsync(ReceiveMessageEvent @event)
    {
        var response = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
            .SendAsync(@event.Message, new List<MicroAIMessage>());
        if (response != null && !response.Content.IsNullOrEmpty())
        {
            if (response.Content.Contains("True"))
            {
                await PublishAsync(new GroupStartEvent() { Message = @event.Message });
            }
        }
    }

    [EventHandler]
    public async Task HandleEventAsync(NamingLogEvent @event)
    {
        switch (@event.Step)
        {
            case NamingContestStepEnum.NamingStart:
                await PublishAsync(new SendMessageEvent()
                {
                    ChatId = _telegramTestOptions.ChatId,
                    Message = $"----the naming contest start now------"
                });
                break;
            case NamingContestStepEnum.Naming:
                await PublishAsync(new SendMessageEvent()
                {
                    ChatId = _telegramTestOptions.ChatId,
                    Message = $"{@event.AgentName} Naming:{@event.Content}"
                });
                break;
            case NamingContestStepEnum.DebateStart:
                await PublishAsync(new SendMessageEvent()
                {
                    ChatId = _telegramTestOptions.ChatId,
                    Message = $"----the naming debate start------"
                });
                break;
            case NamingContestStepEnum.Debate:
                await PublishAsync(new SendMessageEvent()
                {
                    ChatId = _telegramTestOptions.ChatId,
                    Message = $"{@event.AgentName} say:{@event.Content}"
                });
                break;
            case NamingContestStepEnum.Discussion:
                break;
            case NamingContestStepEnum.JudgeVoteStart:
                await PublishAsync(new SendMessageEvent()
                {
                    ChatId = _telegramTestOptions.ChatId,
                    Message = $"----the naming vote start------"
                });
                break;
            case NamingContestStepEnum.JudgeVote:
                await PublishAsync(new SendMessageEvent()
                {
                    ChatId = _telegramTestOptions.ChatId,
                    Message = $"{@event.AgentName}: {@event.Content}"
                });
                break;
            case NamingContestStepEnum.JudgeAsking:
                break;
            case NamingContestStepEnum.JudgeRating:
                break;
            case NamingContestStepEnum.Complete:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}