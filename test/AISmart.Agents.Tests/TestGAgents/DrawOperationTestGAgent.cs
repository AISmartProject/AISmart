using System.ComponentModel;
using System.Runtime.CompilerServices;
using AISmart.Agents;
using AISmart.Application.Grains;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.Grains.Tests.TestGAgents;

[Description("I can draw a polygon")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class DrawOperationTestGAgent : GAgentBase<DrawOperationState, DrawOperateEvent>
{
    public DrawOperationTestGAgent(ILogger logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        throw new NotImplementedException();
    }

    [EventHandler]
    public async Task ExecuteAsync(DrawTriangleTestEvent drawTriangleTestEvent)
    {
        await PublishAsync(new DrawTriangleResultTestEvent()
        {
            DrawTriangleContent = $"I Have draw a {drawTriangleTestEvent.PolygonSides} Triangle"
        });
    }
}

[Serializable]
public class DrawOperationState
{
}

[Serializable]
public class DrawOperateEvent : GEventBase
{
}

[Serializable]
[Description("Draw Triangle")]
public class DrawTriangleTestEvent : EventBase
{
    [Description("Sides of a polygon")]
    public int PolygonSides { get; set; }
}

[Serializable]
public class DrawTriangleResultTestEvent : EventBase
{
    public string DrawTriangleContent { get; set; }
}