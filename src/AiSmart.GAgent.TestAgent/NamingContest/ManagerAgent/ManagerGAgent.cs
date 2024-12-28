using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using AISmart.Events;
using AISmart.GAgent.Core;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.Grains;
using AISmart.Options;
using AISmart.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;

public class ManagerGAgent : GAgentBase<MicroAIGAgentState, AIMessageGEvent>, IManagerGAgent
{
    private readonly NameContestOptions _nameContestOptions;
    

    public ManagerGAgent( ILogger<ManagerGAgent> logger) : base(logger)
    {
    }
    

    public override Task<string> GetDescriptionAsync()
    {
        throw new NotImplementedException();
    }

    public async Task InitAgentsAsync(List<string> agentIdList)
    {
        // RaiseEvent();
        await base.ConfirmEvents();
    }
}