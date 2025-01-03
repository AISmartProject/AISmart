namespace AISmart.Agents.LoadTestAgent;

public class LoadTestMessageCountResult
{
    public bool Success { get; set; }
    public (int Count, DateTime? LastEventTime) AgentCount { get; set; }
    public string Message { get; set; }
}