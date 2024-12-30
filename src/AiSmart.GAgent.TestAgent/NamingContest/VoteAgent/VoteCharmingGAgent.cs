using AISmart.Agent;
using AISmart.Agents;
using AISmart.GAgent.Core;
using AiSmart.GAgent.TestAgent.NamingContest.VoteAgent.Grains;
using Microsoft.Extensions.Logging;

namespace AiSmart.GAgent.TestAgent.NamingContest.VoteAgent;

public class VoteCharmingGAgent : GAgentBase<VoteCharmingState, GEventBase>, IVoteCharmingGAgent
{

    public VoteCharmingGAgent( ILogger<MicroAIGAgent> logger) : base(logger)
    {
    }
    
    [EventHandler]
    public async Task HandleEventAsync(InitVoteCharmingEvent @event)
    {
        Random random = new Random();
        var list = @event.GrainGuidList;
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        base.RaiseEvent(new InitVoteCharmingGEvent
        {
            GrainGuidList = list,
            TotalBatches =  @event.TotalBatches
        });
        await ConfirmEvents();
    }
    
    [EventHandler]
    public async Task HandleEventAsync(VoteCharmingEvent @event)
    {
        if (State.TotalBatches == State.CurrentBatch)
        {
            return;
        }

        int actualBatchSize = 0;
        if (State.TotalBatches == State.CurrentBatch + 1)
        {
            actualBatchSize = State.VoterIds.Count;
        }
        else
        {
            int averageBatchSize = State.VoterIds.Count / State.TotalBatches;
            int minBatchSize = Math.Max(1, (int)(averageBatchSize * 0.6)); 
            int maxBatchSize = Math.Min(State.VoterIds.Count, (int)(averageBatchSize * 1.5)); 
            var random = new Random();
            actualBatchSize = random.Next(minBatchSize, maxBatchSize);
        }

        var selectedVoteIds = State.VoterIds.GetRange(0, actualBatchSize);
        foreach (var voteId in selectedVoteIds)
        {
           await  RegisterAsync(GrainFactory.GetGrain<IMicroAIGAgent>(voteId));
        }

        await PublishAsync(new SingleVoteCharmingEvent
        {
            VoteMessage = @event.VoteMessage,
            Round = @event.Round
        });
        base.RaiseEvent(new VoteCharmingGEvent
        {
            GrainGuidList = selectedVoteIds
        });

        await ConfirmEvents();
    }
    
    
    
    [EventHandler]
    public async Task HandleEventAsync(VoteCharmingCompleteEvent @event)
    {
        await  UnregisterAsync(GrainFactory.GetGrain<IMicroAIGAgent>(@event.VoterId));
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult(
            "Represents an agent responsible for voting charming agents.");
    }
}