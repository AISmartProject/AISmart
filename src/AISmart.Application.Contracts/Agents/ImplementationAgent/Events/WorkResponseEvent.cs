using System.ComponentModel;
using Orleans;

namespace AISmart.Agents.ImplementationAgent.Events;

[Description("Developer Event representing the completion of a task or work item.")]
[GenerateSerializer]
public class WorkCompleteEvent : EventBase
{
    [Description("Details or results of the completed task.")]
    [Id(0)] public string Content { get; set; }
}