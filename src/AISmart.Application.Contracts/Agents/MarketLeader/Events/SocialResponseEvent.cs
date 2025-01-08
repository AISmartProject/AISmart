using System.ComponentModel;
using Orleans;

namespace AISmart.Agents.MarketLeader.Events;

[Description("Social Event representing the completion of a task or work item.")]
[GenerateSerializer]
public class SocialResponseEvent : EventBase
{
    [Description("Details or results of the completed task.")]
    [Id(0)] public string Content { get; set; }
}