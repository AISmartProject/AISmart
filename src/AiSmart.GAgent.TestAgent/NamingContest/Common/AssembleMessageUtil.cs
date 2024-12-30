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

    public static string AssembleSummaryBeforeStep(string summary, string describe)
    {
        return
            $"""
             Based on the summarized experience from the last round, here are the insights:
             {summary}
              
             The next naming theme is:{describe}
             """;
    }
}