using AISmart.Agent;
using AISmart.Agents;
using AISmart.Agent.GEvents;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.Grains;
using AutoGen.Core;
using Microsoft.Extensions.Logging;
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
        var message = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
            .SendAsync(@event.NamingReply,
                new List<MicroAIMessage>()
                {
                    new MicroAIMessage(Role.System.ToString(),
                        $"The theme of this naming contest is: \"{@event.NamingQuestion}\"")
                });
        if (message != null && !message.Content.IsNullOrEmpty())
        {
            var namingReply = message.Content;
            var score = int.Parse(namingReply);
            
        }
    }
}