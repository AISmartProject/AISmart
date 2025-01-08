using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.Application.Grains;
using AISmart.GAgent.Core;
using Microsoft.Extensions.Logging;
using Orleans.Providers;

namespace AISmart.Application.Grains.Agents.Draw;

[Description("I can draw a polygon")]
[StorageProvider(ProviderName = "PubSubStore")]
[LogConsistencyProvider(ProviderName = "LogStorage")]
public class DrawOperationGAgent : GAgentBase<DrawOperationState, DrawOperateEvent>
{
    public DrawOperationGAgent(ILogger<DrawOperationGAgent> logger) : base(logger)
    {
    }

    public override Task<string> GetDescriptionAsync()
    {
        throw new NotImplementedException();
    }

    [EventHandler]
    public async Task<DrawTriangleResultEvent> ExecuteAsync(DrawTriangleEvent drawTriangleEvent)
    {
        return new DrawTriangleResultEvent()
        {
            DrawTriangleContent = $"I Have draw a {drawTriangleEvent.PolygonSides} Triangle"
        };
    }
}

[GenerateSerializer]
public class DrawOperationState : StateBase
{
}

[GenerateSerializer]
[Description("Draw Triangle")]
public class DrawOperateEvent : GEventBase
{
}

[GenerateSerializer]
[Description("Draw Triangle")]
public class DrawTriangleEvent : EventWithResponseBase<DrawTriangleResultEvent>
{
    [Description("Sides of a polygon")]
    [Id(0)] public int PolygonSides { get; set; }
}

[GenerateSerializer]
public class DrawTriangleResultEvent : EventBase
{
    [Id(0)] public string DrawTriangleContent { get; set; }
}