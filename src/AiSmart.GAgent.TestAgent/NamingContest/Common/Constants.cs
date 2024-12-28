namespace AiSmart.GAgent.TestAgent.NamingContest.Common;

public static class NamingConstants
{
    public static string NamingPrompt = "Please answer the naming question above. The name you choose must be different from the names chosen by others. Please directly provide the name you have chosen without any additional content.";
    public static string DebatePrompt = "Please briefly explain why your chosen name is better and why others' names are worse.";
    public static string DiscussionPrompt = "";
    public static string JudgeVotePrompt = """
                                           Based on the content of the naming contest above and the names chosen by the participants, please vote for the name you think is better, along with the concise reason. Please output the name and reason in the following JSON format,and do not include any additional data or characters:
                                           {
                                             "name": "{YourChosenName}",
                                             "reason": "{YourReason}"
                                           }
                                           """;
}