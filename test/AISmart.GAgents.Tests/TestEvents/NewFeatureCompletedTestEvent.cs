using AISmart.Agents;

namespace AISmart.GAgents.Tests.TestEvents;

[GenerateSerializer]
public class NewFeatureCompletedTestEvent : EventBase
{
    [Id(0)] public string PullRequestUrl { get; set; }
}