using System.ComponentModel;
using AISmart.Agents;

namespace AiSmart.GAgent.TestAgent.Voter;


[Description("Vote information")]
[GenerateSerializer]
public class VoterGEvent:EventWithResponseBase<VoteGEventResponse>
{
    [Description("Message for Vote")]
    [Id(0)] public string Message { get; set; }
}