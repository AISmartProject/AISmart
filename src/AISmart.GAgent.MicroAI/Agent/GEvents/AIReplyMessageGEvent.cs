using Orleans;

namespace AISmart.Agent.GEvents;
[GenerateSerializer]
public class AIReplyMessageGEvent : AIMessageGEvent
{
    [Id(0)] public MicroAIMessage Message { get; set; } 
 
}