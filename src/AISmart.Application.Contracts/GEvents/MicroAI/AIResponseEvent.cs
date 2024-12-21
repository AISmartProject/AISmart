using System.ComponentModel;
using AISmart.Agents;
using Orleans;

namespace AISmart.GEvents.MicroAI;
[Description("AI Response.")]
[GenerateSerializer]
public class AIResponseEvent : EventBase
{
    [Description("AI Response Details.")]
    [Id(0)] public string Content { get; set; }
}