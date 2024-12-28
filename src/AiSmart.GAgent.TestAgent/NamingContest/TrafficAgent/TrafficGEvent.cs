using AISmart.Agents;
using AiSmart.GAgent.TestAgent.NamingContest.Common;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

[GenerateSerializer]
public class NamedCompleteGEvent:NameContentGEvent
{
    [Id(0)] public Guid GrainGuid { get; set; }
    [Id(1)] public string CreativeName { get; set; }
    [Id(2)] public string NamingReply { get; set; }
}

[GenerateSerializer]
public class NameContentGEvent:NamingContext
{
    [Id(0)] public string EventId { get; set; }
    [Id(1)] public string AgentId { get; set; }
    [Id(2)] public string AgentName { get; set; }
    [Id(3)] public string EventType { get; set; }

}


[GenerateSerializer]
public class TrafficInformCreativeGEvent:NameContentGEvent
{
    [Id(0)] public string NamingContent { get; set; }
    [Id(1)] public Guid CreativeGrainId { get; set; }
}

[GenerateSerializer]
public class TrafficNamingContestOver : NameContentGEvent
{
    [Id(0)] public string NamingQuestion { get; set; }
}


[GenerateSerializer]
public class TrafficNamingStageOver : NameContentGEvent
{
    [Id(0)] public string NamingQuestion { get; set; }
}



[GenerateSerializer]
public class TrafficInformDebateGEvent:NameContentGEvent
{
    [Id(0)] public string NamingContent { get; set; }
    [Id(1)] public Guid CreativeGrainId { get; set; }
}

[GenerateSerializer]
public class DebatedCompleteGEvent:NameContentGEvent
{
    [Id(0)] public Guid GrainGuid { get; set; }
    [Id(1)] public string CreativeName { get; set; }
    [Id(2)] public string DebateReply { get; set; }
}


[GenerateSerializer]
public class TrafficDebateOver : NameContentGEvent
{
    [Id(0)] public string NamingQuestion { get; set; }
}


