using AISmart.Agents;
using AISmart.Agents.Group;
using AISmart.GAgent.Core;
using Microsoft.Extensions.Logging;

namespace AISmart.Application.Grains.Agents.Group;

[GenerateSerializer]
public class GroupTestGAgentState : StateBase
{
    [Id(0)] public Guid GroupManagerGuid { get; set; }
}

[GAgent]
public class GroupTestGAgent: GAgentBase<GroupTestGAgentState, GroupGEvent>
{
    public GroupTestGAgent(ILogger<GroupTestGAgent> logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        return Task.FromResult("For testing reload group.");
    }
    
    public async Task HandleEventAsync(GroupReloadTestEvent eventData)
    {
        State.GroupManagerGuid = eventData.GroupManagerGuid;
    }
}