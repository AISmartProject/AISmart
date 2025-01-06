using AISmart.GAgent.AtomicAgent.Models;

namespace AISmart.GAgent.AtomicAgent.Dtos;

public class AtomicAgentDto
{
    public string Id { get; set; }
    public AgentType Type { get; set; }
    public string Name { get; set; }
    public AgentPropertyDto Properties { get; set; }
}