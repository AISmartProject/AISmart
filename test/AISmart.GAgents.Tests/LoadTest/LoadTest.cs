using AISmart.Application.Grains.Agents.Group;
using AISmart.Application.Grains.Agents.Publisher;
using AISmart.GGrains.Tests;
using AISmart.Agents.MockA.Events;
using AISmart.Application.Grains.Agents.MockA;
using AISmart.Application.Grains.Agents.MockB;
using AISmart.Application.Grains.Agents.MockC;

namespace AISmart.Grains.Tests.LoadTest;

public class LoadTest : GAgentTestKitBase
{
    [Theory]
    [InlineData(100, 100, 100)]
    public async Task AgentLoadTest(int aGAgentCount, int bGAgentCount, int cGAgentCount)
    {
        var groupGAgent = await Silo.CreateGrainAsync<GroupGAgent>(Guid.NewGuid());
        var publishingAgent = await Silo.CreateGrainAsync<PublishingGAgent>(Guid.NewGuid());

        for (int i = 0; i < aGAgentCount; i++)
        {
            var aGAgent = await Silo.CreateGrainAsync<MockAGAgent>(Guid.NewGuid());

            await groupGAgent.Register(aGAgent);
        }

        for (int i = 0; i < bGAgentCount; i++)
        {
            var bGAgent = await Silo.CreateGrainAsync<MockBGAgent>(Guid.NewGuid());

            await groupGAgent.Register(bGAgent);
        }

        for (int i = 0; i < cGAgentCount; i++)
        {
            var cGAgent = await Silo.CreateGrainAsync<MockCGAgent>(Guid.NewGuid());
            await groupGAgent.Register(cGAgent);
        }

        await publishingAgent.PublishTo(groupGAgent);

        await publishingAgent.PublishEventAsync(new MockAThreadCreatedEvent
        {
            Id = $"mock_A_thread_id",
            Content = $"Testing iteration"
        });
    }
}