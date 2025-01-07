namespace AISmart.GAgent.AtomicAgent.Models;

[GenerateSerializer]
public class AgentData
{
    [Id(0)] public Guid Id { get; set; }
    [Id(1)] public string Type { get; set; }
    [Id(2)] public string Name { get; set; }
    [Id(3)] public string BusinessAgentId { get; set; }
    [Id(4)] public string Properties { get; set; }
}