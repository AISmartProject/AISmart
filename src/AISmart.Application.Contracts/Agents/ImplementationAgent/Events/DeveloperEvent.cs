using System.ComponentModel;
using Orleans;

namespace AISmart.Agents.ImplementationAgent.Events;
[Description("Developer Base Event.")]
[GenerateSerializer]
public class DeveloperEvent : EventBase
{
    [Id(0)] public string Content { get; set; }
}