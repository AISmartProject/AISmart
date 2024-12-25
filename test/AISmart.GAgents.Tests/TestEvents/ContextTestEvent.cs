using AISmart.Agents;

namespace AISmart.GAgents.Tests.TestEvents;

[GenerateSerializer]
public class ContextTestEventBase : EventBase
{
    [Id(0)] public Dictionary<string, object?> ExpectedContext { get; set; }
}

[GenerateSerializer]
public class ContextTestEvent1 : ContextTestEventBase;

[GenerateSerializer]
public class ContextTestEvent2 : ContextTestEventBase;

[GenerateSerializer]
public class ContextTestEvent3 : ContextTestEventBase;