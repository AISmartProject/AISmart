namespace AISmart.GAgent.AtomicAgent.Models;

[GenerateSerializer]
public class AIBasicAgentData
{
    [Id(0)] public string ModelProvider { get; set; }
    [Id(1)] public string Bio { get; set; }
    [Id(2)] public string Lore { get; set; }
    [Id(3)] public string Topic { get; set; }
    [Id(4)] public List<string> KnowledgeBase { get; set; }
}