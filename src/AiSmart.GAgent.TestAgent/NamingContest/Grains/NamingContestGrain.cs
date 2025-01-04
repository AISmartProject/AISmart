using System.Threading.Tasks;
using AISmart.Dto;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.Grains;
using AISmart.Provider;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Orleans;
using Orleans.Providers;

namespace AISmart.Agent.Grains;

[StorageProvider(ProviderName = "PubSubStore")]
public class NamingContestGrain : Grain<NamingContestState>, INamingContestGrain
{
    private readonly INameContestProvider _nameContestProvider;
    
    public NamingContestGrain(INameContestProvider nameContestProvider) 
    {
        _nameContestProvider = nameContestProvider;
    }

    public async Task SendMessageAsync(Guid groupId,NamingLogEvent? nameContentGEvent,string callBackUrl)
    {
        
        if (nameContentGEvent != null)
        {
            await _nameContestProvider.SendMessageAsync(groupId,nameContentGEvent,callBackUrl);
        }
    }
}