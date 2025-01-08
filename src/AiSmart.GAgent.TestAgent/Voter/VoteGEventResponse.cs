using AISmart.Agents;

namespace AiSmart.GAgent.TestAgent.Voter;

[GenerateSerializer]
public class VoteGEventResponse:EventBase
{
    [Id(0)] public string Content { get; set; }
}