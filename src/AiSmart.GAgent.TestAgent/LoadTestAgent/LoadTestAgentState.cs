
namespace AiSmart.GAgent.TestAgent.LoadTestAgent;

[GenerateSerializer]
public class LoadTestAgentState : AISmart.Agents.StateBase
{
    [Id(0)] public List<string> ThreadIds { get; set; }
    [Id(1)] public int Number { get; set; }
    public DateTime StartTimestamp { get; set; }
    public DateTime EndTimestamp { get; set; }

    public void Apply(LoadTestAddNumberEvent @event)
    {
        Number += 1;
    }
}