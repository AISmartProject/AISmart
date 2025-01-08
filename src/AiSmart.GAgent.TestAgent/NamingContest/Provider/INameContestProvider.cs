using System.Threading.Tasks;
using AISmart.Dto;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AiSmart.GAgent.TestAgent.NamingContest.VoteAgent;
using AISmart.PumpFun;

namespace AISmart.Provider;

public interface INameContestProvider
{
    public Task SendMessageAsync(Guid groupId,NamingLogEvent? namingLogEvent,string callBackUrl);
    public Task SendMessageAsync(Guid groupId,VoteCharmingCompleteEvent? namingLogEvent,string callBackUrl);
}