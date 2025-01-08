namespace AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;

public class NamingResponse
{
    public string Name { get; set; }
    public string Reason { get; set; }

    public NamingResponse(string name, string reason)
    {
        Name = name;
        Reason = reason;
    }
}