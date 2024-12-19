using Orleans;

namespace AISmart.Agents.Group;

[GenerateSerializer]
public class GroupAgentState
{
    [Id(0)]  public int RegisteredAgents { get; set; } = 0;
}