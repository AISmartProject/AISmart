using AISmart.GAgent.AtomicAgent.Models;

namespace AISmart.GAgent.AtomicAgent.Dtos;

public class UpdateAtomicAgentDto
{
    public string? Name { get; set; }
    public AgentPropertyDto Properties { get; set; }
}