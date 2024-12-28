namespace AiSmart.GAgent.TestAgent.NamingContest.Common;

public static class NamingConstants
{
    public static string NamingPrompt = "Please answer the naming question above. The name you choose must be different from the names chosen by others. Please directly provide the name you have chosen without any additional content.";
    public static string DebatePrompt = "Please explain why the name you chose is better and why the names chosen by others are worse.";
    public static string DiscussionPrompt = "";
    public static string JudgeVotePrompt = """
                                           Based on the content of the naming contest above and the names chosen by the participants, please vote for the name you think is better, along with the reason. Please output the name and reason in the following JSON format:
                                           {
                                             "name": "{YourChosenName}",
                                             "reason": "{YourReason}"
                                           }
                                           """;
}