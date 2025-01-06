using AISmart.GAgent.AtomicAgent.Models;

namespace AISmart.GAgent.AtomicAgent.Dtos;

public class CreateAtomicAgentDto
{
    public AgentType Type { get; set; }
    public string Name { get; set; }
    public AgentPropertyDto Properties { get; set; }
}