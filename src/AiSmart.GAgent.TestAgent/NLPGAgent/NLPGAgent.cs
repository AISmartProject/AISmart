using System.ComponentModel;
using AISmart.Agent;
using AISmart.Events;
using AiSmart.GAgent.TestAgent.Voter;
using AISmart.GEvents.NLP;
using AISmart.Grains;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AiSmart.GAgent.TestAgent.NLPAgent;

[Description("")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class NLPGAgent : MicroAIGAgent, INLPGAgent
{
    public NLPGAgent(ILogger<MicroAIGAgent> logger) : base(logger)
    {
    }

    public  async Task<NLPGEventResponse> HandleEventAsync(NLPGEvent @event)
    {
        var message = await GrainFactory.GetGrain<IChatAgentGrain>(State.AgentName)
            .SendAsync(@event.Content, State.RecentMessages.ToList());
        
        if (message != null && !message.Content.IsNullOrEmpty())
        {
            var content = message.Content;
            if (message.Content.Contains("Error"))
            {
                content = $"I only handle vote information!!";
            }
            else
            {
                await PublishAsync(new VoterGEvent { Message = message.Content });
            }
            
            
            await PublishAsync(new SendMessageEvent
            {
                Message =$"NLP Reply:{content}" 
            });
        }

        return new NLPGEventResponse();
    }
}