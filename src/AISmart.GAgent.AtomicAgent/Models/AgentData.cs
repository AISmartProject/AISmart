namespace AISmart.GAgent.AtomicAgent.Models;

[GenerateSerializer]
public class AgentData
{
    [Id(0)] public Guid Id { get; set; }
    [Id(1)] public AgentType Type { get; set; }
    [Id(2)] public string Name { get; set; }
    [Id(3)] public AIBasicAgentData AIBasicAgentData { get; set; }
    [Id(4)] public TelegramAgentData TelegramAgentData { get; set; }
    [Id(5)] public TwitterAgentData TwitterAgentData { get; set; }
}