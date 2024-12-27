using System.Threading.Tasks;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using Orleans;

namespace AISmart.Grains;

public interface INamingContestGrain : IGrainWithStringKey
{
    public Task SendMessageAsync(NameContentGEvent nameContentGEvent);
   
}