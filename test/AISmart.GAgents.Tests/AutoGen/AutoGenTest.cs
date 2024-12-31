using AISmart.Agents.AutoGen;
using AISmart.Application.Grains.Agents.Draw;
using AISmart.Application.Grains.Agents.Group;
using AISmart.Application.Grains.Agents.Math;
using AISmart.Application.Grains.Agents.Publisher;
using AISmart.GAgent.Autogen;
using AISmart.GAgent.Autogen.Events;
using AISmart.GAgents.Tests;
using AISmart.Sender;
using Orleans.TestKit;

namespace AISmart.GAgents.Tests.AutoGenTest;

public class AutoGenTest : GAgentTestKitBase
{
    [Fact]
    public async Task AutogenGAgentTest()
    {
        var groupGAgent = await Silo.CreateGrainAsync<GroupGAgent>(Guid.NewGuid());
        var autogenGAgent = await Silo.CreateGrainAsync<AutogenGAgent>(Guid.NewGuid());
        var publishingGAgent = await Silo.CreateGrainAsync<PublishingGAgent>(Guid.NewGuid());
        var drawGAgent = await Silo.CreateGrainAsync<DrawOperationGAgent>(Guid.NewGuid());
        var mathGAgent = await Silo.CreateGrainAsync<MathOperationGAgent>(Guid.NewGuid());
        var autoGenExecutor = await Silo.CreateGrainAsync<AutoGenExecutor>(Guid.NewGuid());

        autogenGAgent.RegisterAgentEvent(typeof(DrawOperationGAgent), [typeof(DrawOperateEvent)]);
        autogenGAgent.RegisterAgentEvent(typeof(MathOperationGAgent), [typeof(AddNumberEvent), typeof(AddNumberResultEvent)]);

        AddProbesByGrainId(autogenGAgent, drawGAgent, mathGAgent, publishingGAgent, groupGAgent);

        await groupGAgent.RegisterAsync(autogenGAgent);
        await groupGAgent.RegisterAsync(drawGAgent);
        await groupGAgent.RegisterAsync(mathGAgent);

        Silo.AddProbe<IPublishingGAgent>(_ => publishingGAgent);
        Silo.AddProbe<IAutoGenExecutor>(_ => autoGenExecutor);

        await publishingGAgent.RegisterAsync(groupGAgent);

        Silo.AddStreamProbe<AutoGenInternalEventBase>();

        await publishingGAgent.PublishEventAsync(new AutoGenCreatedEvent
        {
            Content = "What is 3+3, and then generate the corresponding polygon?"
        });

        await Task.Delay(1000);
    }
}