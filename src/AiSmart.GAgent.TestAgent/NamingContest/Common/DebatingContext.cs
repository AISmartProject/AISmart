using AISmart.Agents;

namespace AiSmart.GAgent.TestAgent.NamingContest.Common;

[GenerateSerializer]
public class DebatingContext:EventBase
{
    [Id(0)]public string Content { get; set; }
}