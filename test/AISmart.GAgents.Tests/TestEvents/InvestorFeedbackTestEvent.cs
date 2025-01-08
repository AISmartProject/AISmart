using AISmart.Agents;

namespace AISmart.GAgents.Tests.TestEvents;

[GenerateSerializer]
public class InvestorFeedbackTestEvent : EventBase
{
    [Id(0)] public string Content { get; set; }
}