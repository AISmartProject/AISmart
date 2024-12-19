using System.ComponentModel;
using Orleans;

namespace AISmart.Agents.ImplementationAgent.Events;

[Description("This class encapsulates data related to development tasks and integrates  with event-driven architectures to handle and respond to these tasks.")]
[GenerateSerializer]
public class ImplementationEvent : EventWithResponseBase<WorkCompleteEvent>
{
    [Description(" the main data or message pertaining to the development task being processed.")]
    [Id(0)] public string Content { get; set; }
}