using AISmart.Agent;
using AISmart.Agents;
using AISmart.Agents.Group;
using AISmart.GAgent.Core;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;
using AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.Sender;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AiSmart.GAgent.TestAgent.NamingContest.VoteAgent;

public class VoteCharmingGAgent : GAgentBase<VoteCharmingState, GEventBase>, IVoteCharmingGAgent
{
    public VoteCharmingGAgent(ILogger<VoteCharmingGAgent> logger) : base(logger)
    {
    }

    [EventHandler]
    public async Task HandleEventAsync(InitVoteCharmingEvent @event)
    {
        if (!State.VoterIds.IsNullOrEmpty())
        {
            return;
        }

        var random = new Random();
        var list = new List<Guid>();
        list.AddRange(@event.CreativeGuidList);
        list.AddRange(@event.JudgeGuidList);

        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = random.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        var grainGuidTypeDictionary = new Dictionary<Guid, string>();
        foreach (var agentId in @event.CreativeGuidList)
        {
            grainGuidTypeDictionary.TryAdd(agentId, NamingConstants.AgentPrefixCreative);
        }

        foreach (var agentId in @event.JudgeGuidList)
        {
            grainGuidTypeDictionary.TryAdd(agentId, NamingConstants.AgentPrefixJudge);
        }

        RaiseEvent(new InitVoteCharmingGEvent
        {
            GrainGuidList = list,
            TotalBatches = @event.TotalBatches,
            Round = @event.Round,
            GrainGuidTypeDictionary = grainGuidTypeDictionary,
            GroupList = @event.groupList,
            TotalGroupCount = @event.TotalGroupCount,
        });

        await ConfirmEvents();
    }

    [EventHandler]
    public async Task HandleEventAsync(VoteCharmingEvent @event)
    {
        Logger.LogInformation(
            "VoteCharmingEvent recieve {info},TotalBatches:{TotalBatches},CurrentBatch:{CurrentBatch}",
            JsonConvert.SerializeObject(@event), State.TotalBatches, State.CurrentBatch);

        if (State.GroupList.Count == 0)
        {
            return;
        }

        var voteGroupList = GetVoteGroupList();
        if (voteGroupList.Count == 0)
        {
            Logger.LogInformation("[VoteCharmingGAgent] VoteCharmingEvent trafficList.Count == 0 ");
        }

        foreach (var groupId in voteGroupList)
        {
            var groupAgent = GrainFactory.GetGrain<IStateGAgent<GroupAgentState>>(groupId);
            var childrenAgent = await groupAgent.GetChildrenAsync();
            var publishAgentId = childrenAgent.FirstOrDefault(f => f.ToString().StartsWith("publishinggagent"));
            IPublishingGAgent publishAgent;
            if (!publishAgentId.IsDefault)
            {
                publishAgent = GrainFactory.GetGrain<IPublishingGAgent>(publishAgentId);
            }
            else
            {
                publishAgent = GrainFactory.GetGrain<IPublishingGAgent>(new Guid());
                await groupAgent.RegisterAsync(publishAgent);
            }

            await publishAgent.PublishEventAsync(new SingleVoteCharmingEvent
            {
                AgentIdNameDictionary = @event.AgentIdNameDictionary,
                VoteMessage = @event.VoteMessage,
                Round = @event.Round,
                VoteCharmingGrainId = this.GetPrimaryKey()
            });

            Logger.LogInformation("SingleVoteCharmingEvent send");
        }

        RaiseEvent(new GroupVoteCompleteSEvent
        {
            VoteGroupList = voteGroupList,
        });

        await ConfirmEvents();
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult(
            "Represents an agent responsible for voting charming agents.");
    }

    public List<Guid> GetVoteGroupList()
    {
        if (State.TotalGroupCount == State.GroupHasVoteCount + 1)
        {
            return State.GroupList;
        }

        var result = new List<Guid>();
        var random = new Random();
        var basicDenominator = Math.Ceiling((double)State.TotalGroupCount / 2);
        var basicNumerator = Math.Abs(basicDenominator - State.GroupList.Count);
        if (basicNumerator / 2 >= random.Next(0, (int)basicNumerator))
        {
            return result;
        }

        var basis = (double)basicNumerator / (double)basicDenominator;
        int randomCount = (int)Math.Ceiling(State.GroupList.Count * (1 - basis));
        randomCount = Math.Max(randomCount / 2, randomCount);
        if (randomCount == 0)
        {
            return result;
        }

        result = State.GroupList.OrderBy(x => random.Next()).Take(randomCount).ToList();
        return result;
    }
}