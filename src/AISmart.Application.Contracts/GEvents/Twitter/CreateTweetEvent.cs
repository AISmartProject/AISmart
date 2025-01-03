using System.ComponentModel;
using AISmart.Agents;
using Orleans;

namespace AISmart.GEvents.Twitter;

[Description("create a tweet")]
[GenerateSerializer]
public class CreateTweetEvent : EventBase
{
    [Description("text content to be post")]
    [Id(0)]  public string Text { get; set; }
}