using System.ComponentModel;
using AISmart.Agents;
using Orleans;

namespace AISmart.GEvents.NLP;

[Description("Handle NlPEvent")]
[GenerateSerializer]
public class NLPGEvent:EventWithResponseBase<NLPGEventResponse>
{
    [Description("Nlp Content")]
    [Id(0)] public string Content { get; set; }
}