using System.Threading.Tasks;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AiSmart.GAgent.TestAgent.NamingContest.VoteAgent;
using Orleans;

namespace AISmart.Grains;

public interface INamingContestGrain : IGrainWithStringKey
{
    public Task SendMessageAsync(Guid groupId,NamingLogEvent? nameContentGEvent,string callBackUrl);
    public Task SendMessageAsync(Guid groupId,VoteCharmingCompleteEvent? nameContentGEvent,string callBackUrl);
   
}