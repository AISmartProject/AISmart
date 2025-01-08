namespace AISmart.Application.Grains.Agents.Developer;

[GenerateSerializer]
public class DeveloperAgentState
{
    [Id(0)]  public List<string> Content { get; set; }
}