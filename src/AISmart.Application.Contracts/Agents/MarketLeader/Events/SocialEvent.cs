using System.ComponentModel;
using Orleans;

namespace AISmart.Agents.MarketLeader.Events;

[Description("Event representing a social interaction that requires a response.")]
[GenerateSerializer]
public class SocialEvent : EventWithResponseBase<SocialResponseEvent>
{
    /// <summary>
    /// Gets or sets the content of the social event. This property contains 
    /// the relevant details or information pertaining to the social event, 
    /// such as messages, event descriptions, or any pertinent data that helps in processing the event.
    /// </summary>
    [Description("Details or information about the social event.")]
    [Id(0)] 
    public string Content { get; set; }
}