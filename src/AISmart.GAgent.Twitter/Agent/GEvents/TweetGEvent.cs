using Orleans;

namespace AISmart.Agent.GEvents;

public class TweetGEvent : Agents.GEventBase
{
    [Id(0)] public string Text { get; set; }
}