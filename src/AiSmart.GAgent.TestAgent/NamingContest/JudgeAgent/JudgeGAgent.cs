using System.Text.Json;
using System.Text.Json.Serialization;
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
using Microsoft.SemanticKernel.Agents;
using Nest;

namespace AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;

public class JudgeGAgent : MicroAIGAgent, IJudgeGAgent
{
    public JudgeGAgent(ILogger<MicroAIGAgent> logger) : base(logger)
    {
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

    [EventHandler]
    public async Task HandleEventAsync(JudgeVoteGEVent @event)
    {
        if (@event.JudgeGrainId != this.GetPrimaryKey())
        {
            return;
        }

        try
        {
            var response = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
                .SendAsync(NamingConstants.JudgeVotePrompt, @event.History);
            if (response != null && !response.Content.IsNullOrEmpty())
            {
                var voteResult = JsonSerializer.Deserialize<VoteResult>(response.Content);
                if (voteResult == null)
                {
                    _logger.LogError("");
                    return;
                }

                await PublishAsync(new JudgeVoteResultGEvent()
                    { AgentName = voteResult.Name, Reason = voteResult.Reason, JudgeGrainId = this.GetPrimaryKey() });
                await PublishAsync(new NamingLogEvent(NamingContestStepEnum.JudgeVote, this.GetPrimaryKey(),
                    NamingRoleType.Judge, State.AgentName, response.Content));
            }
        }
        catch
        {
            // todo:return  JudgeVoteResultGEvent
        }
    }
}

public class VoteResult
{
    [JsonPropertyName(@"name")] public string Name { get; set; }

    [JsonPropertyName(@"reason")] public string Reason { get; set; }
}