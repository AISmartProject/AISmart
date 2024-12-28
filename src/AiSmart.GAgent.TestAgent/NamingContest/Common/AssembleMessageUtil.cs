namespace AiSmart.GAgent.TestAgent.NamingContest.Common;

public static class AssembleMessageUtil
{
    public static string AssembleNamingContent(string agentName, string naming)
    {
        return $"{agentName} named the above information is: \"{naming}\".";
    }

    public static string AssembleDebateContent(string agentName, string debateContent)
    {
        return $"{agentName} debate is: \"{debateContent}\".";
    }
}