using System.Threading.Tasks;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using Orleans;

namespace AISmart.Grains;

public interface INamingContestGrain : IGrainWithStringKey
{
    public Task SendMessageAsync(NamingLogEvent? nameContentGEvent,string callBackUrl);
   
}