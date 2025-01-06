using System.ComponentModel;
using AISmart.Agents;
using AISmart.GAgent.AtomicAgent.Agent.GEvents;
using AISmart.GAgent.AtomicAgent.Models;
using AISmart.GAgent.Core;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.GAgent.AtomicAgent.Agent;

[Description("Handle atomic agent")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class AtomicGAgent : GAgentBase<AtomicGAgentState, AtomicAgentGEvent>, IAtomicGAgent
{
    private readonly ILogger<AtomicGAgent> _logger;

    public AtomicGAgent(ILogger<AtomicGAgent> logger) : base(logger)
    {
        _logger = logger;
    }
    
    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult(
            "Represents an atomic agent responsible for creating other agents such as TwitterGAgent and TelegramGAgent");
    }
    
    public async Task<AgentData?> GetAgentAsync()
    {
        _logger.LogInformation("GetAgentAsync");
        return await Task.FromResult(State.Properties);
    }
    
    public async Task CreateAgentAsync(AgentData data, string address)
    {
        _logger.LogInformation("CreateAgentAsync");
        RaiseEvent(new CreateAgentGEvent()
        {
            Data = data,
            UserAddress = address,
            Id = this.GetPrimaryKey()
        });
        await ConfirmEvents();
    }
    
    public async Task UpdateAgentAsync(AgentData data)
    {
        _logger.LogInformation("UpdateAgentAsync");
        RaiseEvent(new UpdateAgentGEvent()
        {
            Data = data
        });
        await ConfirmEvents();
    }
    
    public async Task DeleteAgentAsync()
    {
        _logger.LogInformation("UpdateAgentAsync");
        RaiseEvent(new DeleteAgentGEvent()
        {
        });
        await ConfirmEvents();
    }
    
    
}

public interface IAtomicGAgent : IStateGAgent<AtomicGAgentState>
{
    Task<AgentData?> GetAgentAsync();
    Task CreateAgentAsync(AgentData data, string address);
    Task UpdateAgentAsync(AgentData data);
    Task DeleteAgentAsync();
}