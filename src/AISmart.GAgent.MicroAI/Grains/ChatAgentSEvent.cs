using AISmart.Agents;
using Orleans;

namespace AISmart.Grains;

[GenerateSerializer]
public class ChatAgentSEvent : GEventBase
{
    [Id(0)] public string AgentResponsibility { get; set; }
    [Id(1)] public string LLM { get; set; } = string.Empty;
}