using System.Text.Json;
using System.Text.Json.Serialization;
using AISmart.Agent;
using AISmart.Agents;
using AISmart.Agent.GEvents;
using AISmart.Events;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.RankingAgent;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AiSmart.GAgent.TestAgent.NamingContest.VoteAgent;
using AISmart.Grains;
using AutoGen.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Agents;
using Nest;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
                var voteResult = JsonSerializer.Deserialize<JudgeVoteChatResponse>(response.Content);
                if (voteResult == null)
                {
                    _logger.LogError("");
                    return;
                }

                await PublishAsync(new JudgeVoteResultGEvent()
                {
                    VoteName = voteResult.Name, Reason = voteResult.Reason, JudgeGrainId = this.GetPrimaryKey(),
                    JudgeName = State.AgentName
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Judge] JudgeVoteGEVent error");
            await PublishAsync(new JudgeVoteResultGEvent()
            {
                VoteName = "", Reason = "", JudgeGrainId = this.GetPrimaryKey(),
                JudgeName = State.AgentName
            });
        }
    }

    [EventHandler]
    public async Task HandleEventAsync(JudgeAskingGEvent @event)
    {
        if (@event.JudgeGuid != this.GetPrimaryKey())
        {
            return;
        }

        var reply = string.Empty;
        try
        {
            var response = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
                .SendAsync(NamingConstants.JudgeAskingPrompt, @event.History);
            if (response != null && !response.Content.IsNullOrEmpty())
            {
                reply = response.Content;
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "[JudgeGAgent] JudgeAskingGEvent error");
        }
        finally
        {
            if (!reply.IsNullOrWhiteSpace())
            {
                await PublishAsync(new NamingLogEvent(NamingContestStepEnum.JudgeAsking, this.GetPrimaryKey(),
                    NamingRoleType.Judge, State.AgentName, reply));
            }

            await PublishAsync(new JudgeAskingCompleteGEvent()
            {
                JudgeGuid = this.GetPrimaryKey(),
                Reply = reply,
            });
        }
    }

    [EventHandler]
    public async Task HandleEventAsync(JudgeScoreGEvent @event)
    {
        var defaultScore = "84.3";
        try
        {
            var response = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
                .SendAsync(NamingConstants.JudgeScorePrompt, @event.History);
            if (response != null && !response.Content.IsNullOrEmpty())
            {
                defaultScore = response.Content;
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "[JudgeGAgent] JudgeScoreGEvent error");
        }
        finally
        {
            if (!defaultScore.IsNullOrWhiteSpace())
            {
                await PublishAsync(new NamingLogEvent(NamingContestStepEnum.JudgeScore, this.GetPrimaryKey(),
                    NamingRoleType.Judge, State.AgentName, defaultScore));
            }

            await PublishAsync(new JudgeScoreCompleteGEvent() { JudgeGrainId = this.GetPrimaryKey() });
        }
    }
    [EventHandler]
    public async Task HandleEventAsync(SingleVoteCharmingEvent @event)
    {
        var history = new List<MicroAIMessage>()
        {
            new MicroAIMessage(Role.User.ToString(),
                $"The theme of this naming contest is: \"{JsonConvert.SerializeObject(@event.VoteMessage)}\""),
        };

        var message = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
            .SendAsync(NamingConstants.VotePrompt, history);

        if (message != null && !message.Content.IsNullOrEmpty())
        {
            var namingReply = message.Content.Replace("\"","");
            var winner = Guid.Parse(namingReply);
                
            await PublishAsync(new VoteCharmingCompleteEvent()
            {
                Winner = winner,
                VoterId = this.GetPrimaryKey(),
                Round = @event.Round
            });
        }
        await base.ConfirmEvents();
    }
}
