using System.ComponentModel;
using Orleans;

namespace AISmart.Agents.Investment;

[Description("Investment Event representing the completion of a task or work item.")]
[GenerateSerializer]
public class InvestmentResponseEvent : EventBase
{
    [Description("Investment Details or results of the completed task.")]
    [Id(0)] public string Content { get; set; }
}