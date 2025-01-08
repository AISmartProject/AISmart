using Orleans;

namespace AISmart.Agents.Group;

[GenerateSerializer]
public class GroupAgentState : StateBase
{
    [Id(0)]  public int RegisteredAgents { get; set; } = 0;
}