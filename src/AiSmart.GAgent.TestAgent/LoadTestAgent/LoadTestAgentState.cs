using AISmart.Application.Grains.Agents.LoadTestAgent;

namespace AISmart.Agents.LoadTestAgent;

[GenerateSerializer]
public class LoadTestAgentState : StateBase
{
    [Id(0)] public List<string> ThreadIds { get; set; }
    [Id(1)] public int Number { get; set; }
    public DateTime LastEventTimestamp { get; set; }

    public void Apply(LoadTestAddNumberEvent @event)
    {
        Number += 1;
    }
}