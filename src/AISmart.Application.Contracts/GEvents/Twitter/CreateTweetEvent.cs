using System.ComponentModel;
using Orleans;

namespace AISmart.GEvents.Twitter;

[Description("create a tweet")]
[GenerateSerializer]
public class CreateTweetEvent
{
    [Description("text content to be post")]
    [Id(0)]  public string Text { get; set; }
}