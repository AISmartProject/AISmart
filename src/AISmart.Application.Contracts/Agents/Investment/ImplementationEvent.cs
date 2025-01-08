using System.ComponentModel;
using Orleans;

namespace AISmart.Agents.Investment;

[Description("Event representing an investment action or occurrence requiring a response.")]
[GenerateSerializer]
public class InvestmentEvent : EventWithResponseBase<InvestmentResponseEvent>
{
    [Description("Details or specifics of the investment event.")]
    [Id(0)] public string Content { get; set; }
}