using AISmart.Agents;
using AISmart.GEvents.MicroAI;
using Json.Schema.Generation;

namespace AiSmart.GAgent.TestAgent.ConclusionAgent;

[Description("Conclusion information")]
[GenerateSerializer]
public class ConclusionGEvent:EventWithResponseBase<ConclusionGEventResponse>
{  
    [Description("Message for summarization")]
    [Id(0)] public string Message { get; set; }
}