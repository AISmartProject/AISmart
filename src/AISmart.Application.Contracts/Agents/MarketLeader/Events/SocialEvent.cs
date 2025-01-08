using Orleans;

namespace AISmart.Agents.MarketLeader.Events;

[GenerateSerializer]
public class SocialEvent : GEvent
{
    [Id(0)] public string Content { get; set; }
}