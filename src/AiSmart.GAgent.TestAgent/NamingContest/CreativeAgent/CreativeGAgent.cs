using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.Grains;
using AutoGen.Core;
using Microsoft.Extensions.Logging;

namespace AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;

public class CreativeGAgent : MicroAIGAgent, ICreativeGAgent
{
    public static readonly string ProposeName = "proposeName";
    public static readonly string Debating = "debating";
    public static readonly string AnswerJudgeQuestions = "answerJudgeQuestions";

    public CreativeGAgent(ILogger<CreativeGAgent> logger) : base(logger)
    {
    }


    [EventHandler]
    public async Task HandleEventAsync(GroupStartEvent @event)
    {
        RaiseEvent(new AIReceiveMessageGEvent()
        {
            Message = new MicroAIMessage(Role.User.ToString(), @event.Message)
        });

        await base.ConfirmEvents();
    }
    
    [EventHandler]
    public async Task HandleEventAsync(TrafficInformCreativeGEvent @event)
    {
        if (@event.CreativeGrainId != this.GetPrimaryKey())
        {
            return;
        }

        var response = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
            .SendAsync(NamingConstants.NamingPrompt, State.RecentMessages.ToList());
        

        if (response != null && !response.Content.IsNullOrEmpty())
        {
            var namingReply = response.Content;

            await this.PublishAsync(new NamedCompleteGEvent()
            {
                Content = @event.NamingContent,
                GrainGuid = this.GetPrimaryKey(),
                NamingReply = namingReply,
                CreativeName = State.AgentName,
            });

            await PublishAsync(new NamingLogEvent(NamingContestStepEnum.Naming, this.GetPrimaryKey(),
                NamingRoleType.Contestant, State.AgentName, namingReply));

            RaiseEvent(new AIReceiveMessageGEvent()
            {
                Message = new MicroAIMessage(Role.User.ToString(),
                    AssembleMessageUtil.AssembleNamingContent(State.AgentName, namingReply))
            });

            await base.ConfirmEvents();
        }
    }

    [EventHandler]
    public async Task HandleEventAsync(NamedCompleteGEvent @event)
    {
        RaiseEvent(new AIReceiveMessageGEvent()
        {
            Message = new MicroAIMessage(Role.User.ToString(),
                AssembleMessageUtil.AssembleNamingContent(@event.CreativeName, @event.NamingReply))
        });
        
        await base.ConfirmEvents();
    }

    [EventHandler]
    public async Task HandleEventAsync(DebatedCompleteGEvent @event)
    {
        RaiseEvent(new AIReceiveMessageGEvent()
        {
            Message = new MicroAIMessage(Role.User.ToString(),
                AssembleMessageUtil.AssembleDebateContent(@event.CreativeName, @event.DebateReply))
        });

        await base.ConfirmEvents();
    }

    [EventHandler]
    public async Task HandleEventAsync(TrafficInformDebateGEvent @event)
    {
        if (@event.CreativeGrainId != this.GetPrimaryKey())
        {
            return;
        }

        var message = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
            .SendAsync(NamingConstants.DebatePrompt, State.RecentMessages.ToList());
        if (message != null && !message.Content.IsNullOrEmpty())
        {
            var debateReply = message.Content;
            await this.PublishAsync(new DebatedCompleteGEvent()
            {
                Content = @event.NamingContent,
                GrainGuid = this.GetPrimaryKey(),
                DebateReply = debateReply,
                CreativeName = State.AgentName,
            });

            RaiseEvent(new AIReceiveMessageGEvent()
            {
                Message = new MicroAIMessage(Role.User.ToString(), AssembleMessageUtil.AssembleDebateContent(State.AgentName, debateReply))
            });

            await PublishAsync(new NamingLogEvent(NamingContestStepEnum.Debate, this.GetPrimaryKey(),
                NamingRoleType.Contestant, State.AgentName, debateReply));

            await base.ConfirmEvents();
        }
    }
}