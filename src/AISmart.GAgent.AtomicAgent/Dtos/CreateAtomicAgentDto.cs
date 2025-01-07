using AISmart.GAgent.AtomicAgent.Models;

namespace AISmart.GAgent.AtomicAgent.Dtos;

public class CreateAtomicAgentDto
{
    public string Type { get; set; }
    public string Name { get; set; }
    public Dictionary<string, string> Properties { get; set; }
}