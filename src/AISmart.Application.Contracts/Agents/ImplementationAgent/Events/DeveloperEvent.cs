using Orleans;

namespace AISmart.Agents.ImplementationAgent.Events;

[GenerateSerializer]
public class DeveloperEvent:GEvent
{
    [Id(0)] public string Content { get; set; }
}