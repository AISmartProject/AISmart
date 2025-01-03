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

    public static string CreativeSummaryHistoryPrompt =
        "Congratulations on winning the last naming contest. Please state the name you chose and provide a brief summary of your winning experience and the lessons learned from your opponent's failure.";

    public static string CreativeDiscussionPrompt = """
                                                    Next, you will collaborate with others in a group to come up with a better name. Within this group, you can discuss the following topics:
                                                    (1) There is a 70% probability of discussing one of the following:
                                                    a. Propose a better name
                                                    b. Discuss the name already proposed
                                                    c. Highlight the advantages or disadvantages of a name suggested by a team member.
                                                    (2) There is a 30% probability of discussing one of the following:
                                                    a. Engage in casual conversation with other group members or reply to their messages.
                                                    Please select one of the topics and directly output its content.
                                                    """;

    public static string TrafficSelectCreativePrompt = "Please select one of the more capable players from the above discussion to summarize the discussion. Only output the selected player's name without any additional information.";

    public static string CreativeGroupSummaryPrompt = """
                                                      Congratulations on being chosen as the leader of this group. 
                                                      You need to select a name from the above discussion or the names suggested by other teammates that you believe represents the group, and briefly explain the reason.
                                                      Please output the name and reason in the following JSON format,and do not include any additional data or characters:
                                                      {
                                                        "name": "{YourChosenName}",
                                                        "reason": "{YourReason}"
                                                      }
                                                      """;

    public static string CreativeGroupSummaryReason = "I think this name is the best.";

    public static string JudgeAskingPrompt = """
                                             You are a judge, and you need to ask a question based on the discussion above. You can only ask one question, and it must not be the same as questions from other judges. The question can be one of the following:
                                             (1) A question about the conclusion.
                                             (2) A question about the connection between the conclusion and real-time events.
                                             (3) A question about the discussion process.
                                             Please directly output your question without any additional information.
                                             """;

    public static string CreativeAnswerQuestionPrompt = "Based on your group's conclusion, please answer the question posed by the previous judge.";
    
    public static string JudgeScorePrompt = """
                                            Please score the name chosen by this group based on the naming theme and the contestants' answers to the judges' questions. The score should be between 1-100, allowing one decimal place. Please output the score directly without any additional information.
                                            """;
        
    public static string SummaryPrompt = "";
    public static string VotePrompt = "The below JSON contains each GUID with their names and associated conversations. Please select the GUID that is most appealing to you,Just return guid";

}
