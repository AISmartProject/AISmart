using AISmart.Agent;
using AISmart.Agents;
using AISmart.Agent.GEvents;
using AISmart.Events;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.RankingAgent;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.Grains;
using AutoGen.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;

public class JudgeGAgent : MicroAIGAgent, IJudgeGAgent
{
    private readonly TelegramTestOptions _telegramTestOptions;

    public JudgeGAgent(IOptions<TelegramTestOptions> options, ILogger<MicroAIGAgent> logger) : base(logger)
    {
        _telegramTestOptions = options.Value;
    }

    [EventHandler]
    public async Task HandleEventAsync(JudgeGEvent @event)
    {
        var history = new List<MicroAIMessage>()
        {
            new MicroAIMessage(Role.User.ToString(),
                $"The theme of this naming contest is: \"{@event.NamingQuestion}\""),
        };
        
        history.AddRange(State.RecentMessages);
        history.Add(new MicroAIMessage(Role.User.ToString(), @event.NamingReply));
        
        List<AIMessageGEvent> sEvent = new List<AIMessageGEvent>();
        sEvent.Add(new AIReceiveMessageGEvent()
            { Message = new MicroAIMessage(Role.User.ToString(), @event.NamingReply) });
        var message = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
            .SendAsync(@event.NamingReply, history);
        
        if (message != null && !message.Content.IsNullOrEmpty())
        {
            var namingReply = message.Content;
            var score = int.Parse(namingReply);

            sEvent.Add(new AIReceiveMessageGEvent()
                { Message = new MicroAIMessage(Role.Assistant.ToString(), namingReply) });
            await PublishAsync(new RankingGEvent()
            {
                CreativeGrainId = @event.CreativeGrainId, CreativeName = @event.CreativeName, Score = score,
                Question = @event.NamingQuestion, Reply = @event.NamingReply
            });


            await PublishAsync(new SendMessageEvent()
            {
                ChatId = _telegramTestOptions.ChatId,
                Message = $"Judge {@event.CreativeName} Naming:{@event.NamingReply} Score:{score}"
            });
        }

        RaiseEvents(sEvent);
        await base.ConfirmEvents();
    }

    [EventHandler]
    public async Task HandleEventAsync(TrafficNamingContestOver @event)
    {
        await PublishAsync(new JudgeOverGEvent() { NamingQuestion = @event.NamingQuestion });
        RaiseEvent(new AIClearMessageGEvent());
        await ConfirmEvents();
    }
}