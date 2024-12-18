using AISmart.Agents;
using AISmart.Agents.AutoGen;
using AISmart.Application.Grains.Agents.Group;
using AISmart.Application.Grains.Agents.Publisher;
using AISmart.GAgent.Autogen;
using AISmart.GAgent.Autogen.Events;
using AISmart.Grains.Tests.TestGAgents;
using AISmart.Sender;
using Orleans.TestKit;

namespace AISmart.Grains.Tests.AutoGenTest;

public class AutoGenTest : GAgentTestKitBase
{
    [Fact]
    public async Task AutogenGAgentTest()
    {
        var groupGAgent = await Silo.CreateGrainAsync<GroupGAgent>(Guid.NewGuid());
        var autogenGAgent = await Silo.CreateGrainAsync<AutogenGAgent>(Guid.NewGuid());
        var publishingGAgent = await Silo.CreateGrainAsync<PublishingGAgent>(Guid.NewGuid());
        var drawGAgent = await Silo.CreateGrainAsync<DrawOperationTestGAgent>(Guid.NewGuid());
        var mathGAgent = await Silo.CreateGrainAsync<MathOperationTestGAgent>(Guid.NewGuid());
        var autoGenExecutor = await Silo.CreateGrainAsync<AutoGenExecutor>(Guid.NewGuid());

        autogenGAgent.RegisterAgentEvent(typeof(DrawOperationTestGAgent), [typeof(DrawTriangleTestEvent)]);
        autogenGAgent.RegisterAgentEvent(typeof(MathOperationTestGAgent), [typeof(AddNumberTestEvent), typeof(SubNumberTestEvent)]);

        AddProbes(autogenGAgent, drawGAgent, mathGAgent, publishingGAgent, groupGAgent);

        await groupGAgent.Register(autogenGAgent);
        await groupGAgent.Register(drawGAgent);
        await groupGAgent.Register(mathGAgent);
        await groupGAgent.Register(groupGAgent);

        Silo.AddProbe<IPublishingGAgent>(_ => publishingGAgent);
        Silo.AddProbe<IAutoGenExecutor>(_ => autoGenExecutor);

        await publishingGAgent.PublishTo(groupGAgent);

        Silo.AddStreamProbe<AutoGenInternalEventBase>();

        await publishingGAgent.PublishEventAsync(new AutoGenCreatedEvent
        {
            Content = "What is 3+3, and then generate the corresponding polygon?"
        });

        await Task.Delay(1000);
    }
}