using System.Text;
using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Events;
using AISmart.GAgent.Core;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using Microsoft.Extensions.Logging;

namespace AiSmart.GAgent.TestAgent.NamingContest.RankingAgent;

public class RankingGAgent : GAgentBase<RankingState, RankingSEventBase>, IRankingGAgent
{
    public RankingGAgent(ILogger<MicroAIGAgent> logger) : base(logger)
    {
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
    public async Task HandleEventAsync(TrafficNamingContestOver @event)
    {
        if (State.RankDic.TryGetValue(@event.NamingQuestion, out var list))
        {
            var sb = new StringBuilder();
            var number = 1;
            foreach (var rankInfo in list)
            {
                sb.AppendLine($"第:{number} 是 {rankInfo.CreativeName} 得分:{rankInfo.Score}");
                number += 1;
            }

            await PublishAsync(new SendMessageEvent() { Message = sb.ToString() });
            
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