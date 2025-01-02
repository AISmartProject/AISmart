namespace AiSmart.GAgent.TestAgent.NamingContest.Common;

public static class NamingConstants
{
    public static string NamingPrompt = "Please answer the naming question above. The name you choose must be different from the names chosen by others. Please directly provide the name you have chosen without any additional content.";
    public static string DebatePrompt = "Please refute others' opinions and explain why their chosen names are not good and why your chosen name is better. Keep your response concise.";
    public static string DiscussionPrompt = "";
    public static string JudgeVotePrompt = """
                                           Based on the content of the naming contest above and the names chosen by the participants, please vote for the name you think is better, along with the concise reason. Please output the name and reason in the following JSON format,and do not include any additional data or characters:
                                           {
                                             "name": "{YourChosenName}",
                                             "reason": "{YourReason}"
                                           }
                                           """;


    public static string DefaultDebateContent = "I believe the name I chose is the best.";
    public static string DefaultCreativeNaming = "Nick";

    public static string CreativeSummary =
        "Congratulations on winning the last naming contest. Please state the name you chose and provide a brief summary of your winning experience and the lessons learned from your opponent's failure.";

    public static string CreativeDiscussionPrompt = "";
    public static string VotePrompt = "The below JSON contains each GUID with their names and associated conversations. Please select the GUID that is most appealing to you";

}