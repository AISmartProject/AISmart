using AISmart.Agents;

namespace AiSmart.GAgent.TestAgent.NamingContest.JudgeAgent;

[GenerateSerializer]
public class JudgeCloneSEvent:GEventBase
{
    [Id(0)] public Guid JudgeGrainId { get; set; }
}