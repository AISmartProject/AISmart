using AISmart.Agents;
using AISmart.Dapr;
using AISmart.Provider;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.SemanticKernel;
using Newtonsoft.Json;
using Orleans.Streams;

namespace AiSmart.GAgent.TestAgent.NamingContest.VoteAgent.Grains;

public class VoteAegntGrain : Grain,IVoteAgentGrain
{
    private IStreamProvider StreamProvider => this.GetStreamProvider(CommonConstants.StreamProvider);
    private MiddlewareStreamingAgent<SemanticKernelAgent>? _agent;
    private IAIAgentProvider _aiAgentProvider;
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