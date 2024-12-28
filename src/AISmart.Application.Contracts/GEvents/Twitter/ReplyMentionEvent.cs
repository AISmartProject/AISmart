using System.ComponentModel;
using AISmart.Agents;
using Orleans;

namespace AISmart.GEvents.Twitter;

[Description("reply mention in tweet.")]
[GenerateSerializer]
public class ReplyMentionEvent : EventBase
{
    
}