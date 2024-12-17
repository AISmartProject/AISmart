using AISmart.Application.Grains.Command;
using AISmart.Cqrs.Command;

namespace AISmart.Application.Grains.Agents.Developer;

[GenerateSerializer]
public class DeveloperAgentState : BaseState
{
    [Id(0)]  public List<string> Content { get; set; }
}

public class DeveloperAgentCommand : BaseCommand<DeveloperAgentState>
{
    
}