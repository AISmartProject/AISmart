using AISmart.Agents;
using AiSmart.GAgent.TestAgent.NamingContest.Common;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

[GenerateSerializer]
public class NamedCompleteGEvent:NamingContext
{
    [Id(0)] public Guid GrainGuid { get; set; }
    [Id(1)] public string CreativeName { get; set; }
    [Id(2)] public string NamingReply { get; set; }
}

[GenerateSerializer]
public class TrafficInformCreativeGEvent:GEventBase
{
    [Id(0)] public string NamingContent { get; set; }
}

[GenerateSerializer]
public class TrafficNamingContestOver : EventBase
{
    [Id(0)] public string NamingQuestion { get; set; }
}