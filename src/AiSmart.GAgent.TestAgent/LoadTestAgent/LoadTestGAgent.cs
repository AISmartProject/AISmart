using System.Diagnostics.CodeAnalysis;
using AISmart.Agents;
using AISmart.Agents.LoadTestAgent;
using AISmart.GAgent.Core;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.Application.Grains.Agents.LoadTestAgent;

[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class LoadTestGAgent : GAgentBase<LoadTestAgentState, LoadTestGEvent>, ILoadTestGAgentCount
{
    public LoadTestGAgent(ILogger<LoadTestGAgent> logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("An agent to inform other agents when an A thread is published.");
    }


    [EventHandler]
    public async Task HandleEventAsync(NamingLogEvent @event)
    {
        Logger.LogInformation($"{GetType()} ExecuteAsync: LoadTestGAgent analyses content: {@event}");
        
        switch (@event.Step)
        {
            case NamingContestStepEnum.Complete:
                State.LastEventTimestamp = DateTime.UtcNow;
                RaiseEvent(new LoadTestAddNumberEvent());

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public Task<(int Number, DateTime LastEventTimestamp)> GetLoadTestGAgentCount()
    {
        return Task.FromResult((State.Number, State.LastEventTimestamp));
    }
}