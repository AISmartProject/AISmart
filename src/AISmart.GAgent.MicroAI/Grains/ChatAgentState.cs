using Orleans;

namespace AISmart.Grains;

[GenerateSerializer]
public class ChatAgentState : StateBase
{
    [Id(0)] public string AgentResponsibility { get; set; }
    [Id(1)] public string LLm { get; set; }

    public void Apply(ChatAgentSEvent @event)
    {
        AgentResponsibility = @event.AgentResponsibility;
        LLm = @event.LLM;
    }
}