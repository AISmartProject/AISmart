using System.Text;
using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Events;
using AISmart.GAgent.Core;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiSmart.GAgent.TestAgent.NamingContest.RankingAgent;

public class RankingGAgent : GAgentBase<RankingState, RankingSEventBase>, IRankingGAgent
{
    private readonly TelegramTestOptions _telegramTestOptions;

    public RankingGAgent(IOptions<TelegramTestOptions> options, ILogger<MicroAIGAgent> logger) : base(logger)
    {
        _telegramTestOptions = options.Value;
    }


    [EventHandler]
    public async Task HandleEventAsync(RankingGEvent @event)
    {
        base.RaiseEvent(new RankingSEvent
        {
            CreativeGrainId = @event.CreativeGrainId, Reply = @event.Reply, Score = @event.Score,
            Question = @event.Question, CreativeName = @event.CreativeName
        });

        await base.ConfirmEvents();
    }

    [EventHandler]
    public async Task HandleEventAsync(JudgeOverGEvent @event)
    {
        if (State.RankDic.TryGetValue(@event.NamingQuestion, out var list))
        {
            var sb = new StringBuilder();
            sb.AppendLine("Ranking Result:");
            var number = 1;
            foreach (var rankInfo in list)
            {
                sb.AppendLine(
                    $"Ranking:{number} AgentName:{rankInfo.CreativeName} Score:{rankInfo.Score}  Naming:{rankInfo.Reply}");
                number += 1;
            }

            await PublishAsync(new SendMessageEvent()
                { ChatId = _telegramTestOptions.ChatId, Message = sb.ToString() });

            base.RaiseEvent(new RankingCleanSEvent
            {
                Question = @event.NamingQuestion
            });

            await base.ConfirmEvents();
        }
    }

    public override Task<string> GetDescriptionAsync()
    {
        throw new NotImplementedException();
    }
}