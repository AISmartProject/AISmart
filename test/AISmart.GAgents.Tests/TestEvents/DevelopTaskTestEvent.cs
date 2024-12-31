using AISmart.Agents;

namespace AISmart.GAgents.Tests.TestEvents;

[GenerateSerializer]
public class DevelopTaskTestEvent : EventWithResponseBase<NewFeatureCompletedTestEvent>
{
    [Id(0)] public string Description { get; set; }
}