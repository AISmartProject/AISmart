using Orleans;

namespace AISmart.Agent.GEvents;

public class BindTwitterAccountGEvent : TweetGEvent
{
    [Id(0)] public string UserId { get; set; }
    [Id(1)] public string Token { get; set; }
    [Id(2)] public string TokenSecret { get; set; }
}