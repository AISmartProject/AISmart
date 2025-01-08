using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.Application.Grains;
using AISmart.GAgent.Core;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;

namespace AISmart.Application.Grains.Agents.Math;

[Description("i can add two integer")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class MathOperationGAgent : GAgentBase<MathOperationState, MathOperationEvent>
{
    public MathOperationGAgent(ILogger<MathOperationGAgent> logger) : base(logger)
    {
    }

    [EventHandler]
    public async Task<AddNumberResultEvent> ExecuteAsync(AddNumberEvent numberEvent)
    {
        return new AddNumberResultEvent()
        {
            Total = numberEvent.B + numberEvent.A + 1
        };
    }

    public override Task<string> GetDescriptionAsync()
    {
        throw new NotImplementedException();
    }
}

[GenerateSerializer]
public class MathOperationState : StateBase
{
}

public class MathOperationEvent : GEventBase
{
}

[GenerateSerializer]
[Description("add Two number")]
[assembly: InternalsVisibleTo("AnotherAssembly")]
public class AddNumberEvent : EventWithResponseBase<AddNumberResultEvent>
{
    [Description("First Number")] [Id(0)] public int A { get; set; }
    [Description("Second Number")] [Id(1)] public int B { get; set; }
}

[GenerateSerializer]
[Description("Sub Two number")]
[assembly: InternalsVisibleTo("AnotherAssembly")]
public class SubNumberEvent : EventBase
{
    [Description("First Number")] [Id(0)] public int A { get; set; }
    [Description("Second Number")] [Id(1)] public int B { get; set; }
}

[GenerateSerializer]
[Description("Add Two number's Result")]
public class AddNumberResultEvent : EventBase
{
    [Id(0)] public int Total { get; set; }
}