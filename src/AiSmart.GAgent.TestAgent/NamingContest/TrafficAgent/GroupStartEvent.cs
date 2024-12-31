using System.ComponentModel;
using AISmart.Agents;
using Orleans;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

[Description("tell group to start")]
[GenerateSerializer]
public class GroupStartEvent : EventBase
{
    [Description("Unique identifier for the  message.")]
    [Id(0)]  public string MessageId { get; set; }
    
    [Description("Text content of the  message.")]
    [Id(1)] public string Message { get; set; }
}