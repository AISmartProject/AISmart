using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Events;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.Grains;
using AISmart.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;

public class CreativeGAgent : MicroAIGAgent, ICreativeGAgent
{
    private readonly NameContestOptions _nameContestOptions;

    public static readonly string ProposeName = "proposeName";
    public static readonly string Debating = "debating";
    public static readonly string AnswerJudgeQuestions = "answerJudgeQuestions";

    public CreativeGAgent(IOptions<NameContestOptions> options, ILogger<MicroAIGAgent> logger) : base(logger)
    {
        _nameContestOptions = options.Value;
    }

    [EventHandler]
    public async Task HandleEventAsync(TrafficInformCreativeGEvent @event)
    {
        if (@event.CreativeGrainId != this.GetPrimaryKey())
        {
            return;
        }

        IChatAgentGrain chatAgentGrain =
            GrainFactory.GetGrain<IChatAgentGrain>(_nameContestOptions.CreativeGAgent[@event.GetType().Name]);
            
        var message = await chatAgentGrain.SendAsync(_nameContestOptions.CreativeGAgent[@event.GetType().Name], new List<MicroAIMessage>());
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
        }
    }

    [EventHandler]
    public async Task HandleEventAsync(TrafficInformDebateGEvent @event)
    {
        if (@event.CreativeGrainId != this.GetPrimaryKey())
        {
            return;
        }

        var message = await GrainFactory.GetGrain<IChatAgentGrain>(this.GetPrimaryKey() + Debating)
            .SendAsync(@event.NamingContent, new List<MicroAIMessage>());
        if (message != null && !message.Content.IsNullOrEmpty())
        {
            var namingReply = message.Content;

            await this.PublishAsync(new DebatedCompleteGEvent()
            {
                Content = @event.NamingContent,
                GrainGuid = this.GetPrimaryKey(),
                NamingReply = namingReply,
                CreativeName = State.AgentName,
            });
        }
    }
}