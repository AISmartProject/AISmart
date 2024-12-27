using AISmart.Agents;
using Nest;

namespace AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;

[GenerateSerializer]
public class TrafficEventSourcingBase : GEventBase
{
}

[GenerateSerializer]
public class TrafficCallSelectCreativeSEvent : TrafficEventSourcingBase
{
    public Guid CreativeGrainId { get; set; }
}

[GenerateSerializer]
public class TrafficCreativeFinishSEvent : TrafficEventSourcingBase
{
    [Id(0)] public Guid CreativeGrainId { get; set; }
}

[GenerateSerializer]
public class TrafficCreativeCompleteGEvent : TrafficEventSourcingBase
{
    [Id(0)] public Guid CompleteGrainId { get; set; }
}


[GenerateSerializer]
public class TrafficDebateCompleteGEvent : TrafficEventSourcingBase
{
    [Id(0)] public Guid CompleteGrainId { get; set; }
}



[GenerateSerializer]
public class TrafficNameStartSEvent:TrafficEventSourcingBase
{
    [Id(0)] public string Content { get; set; }
}

[GenerateSerializer]
public class TrafficSetAgentSEvent:TrafficEventSourcingBase
{
    [Id(0)] public string AgentName { get; set; }
    [Id(1)] public string Description { get; set; }
}

[GenerateSerializer]
public class AddCreativeAgent : TrafficEventSourcingBase
{
    [Id(0)] public Guid CreativeGrainId { get; set; }
}