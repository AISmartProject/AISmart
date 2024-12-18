using System.ComponentModel;
using System.Runtime.CompilerServices;
using AISmart.Agents;
using AISmart.Application.Grains;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.Grains.Tests.TestGAgents;

[Description("i can add two integer")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class MathOperationTestGAgent: GAgentBase<MathOperationTestGAgentState, MathOperationTestGEvent>
{
    public MathOperationTestGAgent(ILogger logger) : base(logger)
    {
    }
    
    [EventHandler]
    public async Task ExecuteAsync(AddNumberTestEvent numberTestEvent)
    {
        await PublishAsync(new AddNumberResultTestEvent()
        {
            Total = numberTestEvent.B + numberTestEvent.A + 1
        });
    }

    public override Task<string> GetDescriptionAsync()
    {
        throw new NotImplementedException();
    }
}

[GenerateSerializer]
public class MathOperationTestGAgentState
{
}

public class MathOperationTestGEvent : GEventBase
{
}

[Serializable]
[Description("add Two number")]
public class AddNumberTestEvent : EventBase
{
    [Description("First Number")]
    public int A { get; set; }
    [Description("Second Number")]
    public int B { get; set; }
}

[Serializable]
[Description("Sub Two number")]
public class SubNumberTestEvent : EventBase
{
    [Description("First Number")]
    public int A { get; set; }
    [Description("Second Number")]
    public int B { get; set; }
}

[Serializable]
public class AddNumberResultTestEvent : EventBase
{
    public int Total { get; set; }
}