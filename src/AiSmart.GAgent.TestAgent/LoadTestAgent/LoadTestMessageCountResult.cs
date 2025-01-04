namespace AiSmart.GAgent.TestAgent.LoadTestAgent;

public class LoadTestMessageCountResult
{
    public bool Success { get; set; }
    public int AgentCount { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string Message { get; set; }
}