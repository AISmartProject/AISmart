using System.Diagnostics.CodeAnalysis;
using AISmart.Agents;
using AISmart.GAgent.Core;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AiSmart.GAgent.TestAgent.LoadTestAgent;

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
            case NamingContestStepEnum.NamingStart:
                State.StartTimestamp = DateTime.UtcNow;
                break;
            case NamingContestStepEnum.Naming:
                break;
            case NamingContestStepEnum.DebateStart:
                break;
            case NamingContestStepEnum.Debate:
                break;
            case NamingContestStepEnum.Discussion:
                break;
            case NamingContestStepEnum.JudgeVoteStart:
                break;
            case NamingContestStepEnum.JudgeVote:
                break;
            case NamingContestStepEnum.JudgeAsking:
                break;
            case NamingContestStepEnum.Complete:
                State.EndTimestamp = DateTime.UtcNow;
                RaiseEvent(new LoadTestAddNumberEvent());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public Task<(int Number, DateTime StartTimestamp, DateTime EndTimestamp)> GetLoadTestGAgentInfo()
    {
        return Task.FromResult((State.Number, State.StartTimestamp, State.EndTimestamp));
    }
}