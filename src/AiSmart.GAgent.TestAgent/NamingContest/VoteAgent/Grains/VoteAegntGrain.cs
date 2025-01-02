using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Dapr;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AISmart.Provider;
using AutoGen.Core;
using AutoGen.SemanticKernel;
using Newtonsoft.Json;
using Orleans.Streams;

namespace AiSmart.GAgent.TestAgent.NamingContest.VoteAgent.Grains;

public class VoteAegntGrain : Grain,IVoteAgentGrain
{
    private IStreamProvider StreamProvider => this.GetStreamProvider(CommonConstants.StreamProvider);
    private MiddlewareStreamingAgent<SemanticKernelAgent>? _agent;
    private IAIAgentProvider _aiAgentProvider;

    public VoteAegntGrain(IAIAgentProvider aiAgentProvider
        )
    {
        _aiAgentProvider = aiAgentProvider;
    }

    public async Task VoteAgentAsync(VoteCharmingEvent voteCharmingEvent)
    {
      var microAIMessage  = await _aiAgentProvider.SendAsync(_agent, JsonConvert.ToString(voteCharmingEvent)+"  The above JSON contains each GUID with their names and associated conversations. Please select the GUID that is most appealing to you", null);
      if (microAIMessage.Content != null)
      {
          await PublishEventAsync(new VoteCharmingCompleteEvent
          {
              Winner = Guid.Parse(microAIMessage.Content),
              VoterId = this.GetPrimaryKey(),
              Round = 1
          });
      }
    }

    public async Task VoteAgentAsync(SingleVoteCharmingEvent singleVoteCharmingEvent)
    {
        var historyMessage = "The theme of this naming contest is:" + JsonConvert.SerializeObject(@singleVoteCharmingEvent.VoteMessage);

        var history = new List<MicroAIMessage>()
        {
            new MicroAIMessage(Role.User.ToString(), historyMessage),
        };
        var message  = await _aiAgentProvider.SendAsync(_agent, NamingConstants.VotePrompt,history);
        if (message.Content != null)
        {
            var namingReply = message.Content.Replace("\"","");
            var winner = Guid.Parse(namingReply);
            await PublishEventAsync(new VoteCharmingCompleteEvent
            {
                Winner = Guid.Parse(namingReply),
                VoterId = this.GetPrimaryKey(),
                Round = 1
            });
        }
    }

    private async Task PublishEventAsync(EventBase publishData)
    {
        var streamId = StreamId.Create(CommonConstants.StreamNamespace, this.GetPrimaryKey());
        var stream = StreamProvider.GetStream<EventBase>(streamId);
        await stream.OnNextAsync(publishData);
    }

    public override async Task<Task> OnActivateAsync(CancellationToken cancellationToken)
    {
        _agent = await _aiAgentProvider.GetAgentAsync("", "");
        return base.OnActivateAsync(cancellationToken);
    }
}