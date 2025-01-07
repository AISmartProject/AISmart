using AISmart.GAgent.AtomicAgent.Models;

namespace AISmart.GAgent.AtomicAgent.Dtos;

public class UpdateAtomicAgentDto
{
    public string? Name { get; set; }
    public Dictionary<string, string> Properties { get; set; }
}