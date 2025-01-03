using System.Threading.Tasks;
using AISmart.Dto;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.PumpFun;

namespace AISmart.Provider;

public interface INameContestProvider
{
    public Task SendMessageAsync(NameContentGEvent nameContentGEvent,string callBackUrl);
}