using AISmart.Agents;
using Orleans;

namespace AISmart.Agent.GEvents;
[GenerateSerializer]
public class AIReceiveMessageGEvent : AIMessageGEvent
{
    [Id(0)] public MicroAIMessage Message { get; set; }
}