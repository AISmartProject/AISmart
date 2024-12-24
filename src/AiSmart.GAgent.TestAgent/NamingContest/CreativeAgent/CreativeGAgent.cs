using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Events;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.Grains;
using Microsoft.Extensions.Logging;

namespace AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;

public class CreativeGAgent : MicroAIGAgent, ICreativeGAgent
{
    public CreativeGAgent(ILogger<MicroAIGAgent> logger) : base(logger)
    {
    }

    [EventHandler]
    public async Task HandleEventAsync(TrafficInformCreativeGEvent @event)
    {
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
            });
        }
    }
}