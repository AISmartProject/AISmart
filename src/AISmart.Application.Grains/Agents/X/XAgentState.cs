namespace AISmart.Application.Grains.Agents.X;

[GenerateSerializer]
public class XAgentState
{
    [Id(0)] public List<string> ThreadIds { get; set; }
}