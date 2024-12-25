using AISmart.Application.Grains.Agents.Group;
using AISmart.GAgent.Core;
using AISmart.GAgents.Tests.TestGAgents;
using Shouldly;

namespace AISmart.GAgents.Tests.GAgentBase;

[Trait("Category", "BVT")]
public class GroupingTests : GAgentTestKitBase
{
    [Fact(DisplayName = "GroupGAgent should be initialized correctly.")]
    public async Task InitGroupGAgentTest()
    {
        // Arrange & Act.
        var groupGAgent = await Silo.CreateGrainAsync<GroupGAgent>(Guid.NewGuid());

        // Assert: GroupGAgent itself will be the publisher.
        var publishers = new GrainState<Dictionary<Guid, string>>();
        await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.PublishersStateName,
            groupGAgent.GetGrainId(),
            publishers);
        publishers.State.Count.ShouldBe(1);
        publishers.State.First().Value.ShouldContain(groupGAgent.GetPrimaryKey().ToString());

        // Assert: Subscribers should be empty because no member is registered.
        var subscribers = new GrainState<List<GrainId>>();
        await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscribersStateName,
            groupGAgent.GetGrainId(),
            subscribers);
        subscribers.State.Count.ShouldBe(0);
    }

    [Fact(DisplayName = "Each gAgent's states should be saved correctly after register.")]
    public async Task RegisterTest()
    {
        // Arrange & Act.
        var naiveTestGAgent = await Silo.CreateGrainAsync<NaiveTestGAgent>(Guid.NewGuid());
        var groupGAgent = await CreateGroupGAgentAsync(naiveTestGAgent);

        // Assert: Check group member's states from GrainStorage.
        {
            var subscriptions = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscriptionsStateName,
                naiveTestGAgent.GetGrainId(),
                subscriptions);
            subscriptions.State.Count.ShouldBe(1);
            subscriptions.State.First().Value.ShouldContain(groupGAgent.GetPrimaryKey().ToString());

            var publishers = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.PublishersStateName,
                naiveTestGAgent.GetGrainId(),
                publishers);
            publishers.State.Count.ShouldBe(2);
            publishers.State.First().Value.ShouldContain(naiveTestGAgent.GetPrimaryKey().ToString());
            publishers.State.Last().Value.ShouldContain(groupGAgent.GetPrimaryKey().ToString());
        }

        // Assert: Check group's states from GrainStorage.
        {
            var publishers = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.PublishersStateName,
                groupGAgent.GetGrainId(),
                publishers);
            publishers.State.Count.ShouldBe(1);
            publishers.State.First().Value.ShouldContain(groupGAgent.GetPrimaryKey().ToString());

            var subscribers = new GrainState<List<GrainId>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscribersStateName,
                groupGAgent.GetGrainId(),
                subscribers);
            subscribers.State.Count.ShouldBe(1);
            subscribers.State.First().ShouldBe(naiveTestGAgent.GetGrainId());
        }
    }

    [Fact(DisplayName = "Each gAgent's states should be saved correctly after unregister.")]
    public async Task UnregisterTest()
    {
        // Arrange.
        var naiveTestGAgent = await Silo.CreateGrainAsync<NaiveTestGAgent>(Guid.NewGuid());
        var groupGAgent = await CreateGroupGAgentAsync(naiveTestGAgent);

        // Act.
        await groupGAgent.UnregisterAsync(naiveTestGAgent);

        // Assert: Check member's states from GrainStorage.
        {
            var subscriptions = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscriptionsStateName,
                naiveTestGAgent.GetGrainId(),
                subscriptions);
            subscriptions.State.Count.ShouldBe(0);

            var publishers = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.PublishersStateName,
                naiveTestGAgent.GetGrainId(),
                publishers);
            // Only itself.
            publishers.State.Count.ShouldBe(1);
        }

        // Assert: Check group's states from GrainStorage.
        {
            var publishers = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.PublishersStateName,
                groupGAgent.GetGrainId(),
                publishers);
            // GroupGAgent itself will still be the publisher.
            publishers.State.Count.ShouldBe(1);
            publishers.State.First().Value.ShouldContain(groupGAgent.GetPrimaryKey().ToString());

            var subscribers = new GrainState<List<GrainId>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscribersStateName,
                groupGAgent.GetGrainId(),
                subscribers);
            subscribers.State.Count.ShouldBe(0);
        }
    }

    [Fact(DisplayName = "Multiple gAgents should be registered correctly to one group.")]
    public async Task OneGroupRegisterMultipleGAgentsTest()
    {
        // Arrange & Act.
        var naiveTestGAgent1 = await Silo.CreateGrainAsync<NaiveTestGAgent>(Guid.NewGuid());
        var naiveTestGAgent2 = await Silo.CreateGrainAsync<NaiveTestGAgent>(Guid.NewGuid());
        var naiveTestGAgent3 = await Silo.CreateGrainAsync<NaiveTestGAgent>(Guid.NewGuid());
        var groupGAgent = await CreateGroupGAgentAsync(naiveTestGAgent1, naiveTestGAgent2, naiveTestGAgent3);

        // Assert: Check each member's states from GrainStorage.
        foreach (var gAgent in new List<NaiveTestGAgent> { naiveTestGAgent1, naiveTestGAgent2, naiveTestGAgent3 })
        {
            var subscriptions = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscriptionsStateName,
                gAgent.GetGrainId(),
                subscriptions);
            subscriptions.State.Count.ShouldBe(1);
            subscriptions.State.First().Value.ShouldContain(groupGAgent.GetPrimaryKey().ToString());

            var publishers = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.PublishersStateName,
                gAgent.GetGrainId(),
                publishers);
            publishers.State.Count.ShouldBe(2);
            publishers.State.First().Value.ShouldContain(gAgent.GetPrimaryKey().ToString());
            publishers.State.Last().Value.ShouldContain(groupGAgent.GetPrimaryKey().ToString());
        }

        // Assert: Check group's states from GrainStorage.
        {
            var publishers = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.PublishersStateName,
                groupGAgent.GetGrainId(),
                publishers);
            publishers.State.Count.ShouldBe(1);
            publishers.State.First().Value.ShouldContain(groupGAgent.GetPrimaryKey().ToString());

            var subscribers = new GrainState<List<GrainId>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscribersStateName,
                groupGAgent.GetGrainId(),
                subscribers);
            subscribers.State.Count.ShouldBe(3);
        }
    }

    [Fact(DisplayName = "One gAgent should be registered correctly to multiple group.")]
    public async Task MultipleGroupRegisterOneGAgentTest()
    {
        // Arrange & Act.
        var naiveTestGAgent = await Silo.CreateGrainAsync<NaiveTestGAgent>(Guid.NewGuid());
        var groupGAgent1 = await CreateGroupGAgentAsync(naiveTestGAgent);
        var groupGAgent2 = await CreateGroupGAgentAsync(naiveTestGAgent);
        var groupGAgent3 = await CreateGroupGAgentAsync(naiveTestGAgent);

        // Assert: Check member's states from GrainStorage.
        {
            var subscriptions = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscriptionsStateName,
                naiveTestGAgent.GetGrainId(),
                subscriptions);
            subscriptions.State.Count.ShouldBe(3);

            var publishers = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.PublishersStateName,
                naiveTestGAgent.GetGrainId(),
                publishers);
            publishers.State.Count.ShouldBe(4);
        }

        // Assert: Check each group's states from GrainStorage.
        foreach (var groupGAgent in new List<GroupGAgent> { groupGAgent1, groupGAgent2, groupGAgent3 })
        {
            var publishers = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.PublishersStateName,
                groupGAgent.GetGrainId(),
                publishers);
            publishers.State.Count.ShouldBe(1);
            publishers.State.First().Value.ShouldContain(groupGAgent.GetPrimaryKey().ToString());

            var subscribers = new GrainState<List<GrainId>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscribersStateName,
                groupGAgent.GetGrainId(),
                subscribers);
            subscribers.State.Count.ShouldBe(1);
            subscribers.State.First().ShouldBe(naiveTestGAgent.GetGrainId());
        }
    }

    [Fact(DisplayName = "One gAgent should be unregistered correctly from multiple group.")]
    public async Task MultipleGroupRegisterAndUnregisterOneGAgentTest()
    {
        // Arrange.
        var naiveTestGAgent = await Silo.CreateGrainAsync<NaiveTestGAgent>(Guid.NewGuid());
        var groupGAgent1 = await CreateGroupGAgentAsync(naiveTestGAgent);
        var groupGAgent2 = await CreateGroupGAgentAsync(naiveTestGAgent);
        var groupGAgent3 = await CreateGroupGAgentAsync(naiveTestGAgent);

        // Act.
        await groupGAgent1.UnregisterAsync(naiveTestGAgent);

        // Assert: Check member's states from GrainStorage.
        {
            var subscriptions = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscriptionsStateName,
                naiveTestGAgent.GetGrainId(),
                subscriptions);
            subscriptions.State.Count.ShouldBe(2);

            var publishers = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.PublishersStateName,
                naiveTestGAgent.GetGrainId(),
                publishers);
            publishers.State.Count.ShouldBe(3);
        }

        // Assert: Check groupGAgent1's states from GrainStorage.
        {
            var publishers = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.PublishersStateName,
                groupGAgent1.GetGrainId(),
                publishers);
            publishers.State.Count.ShouldBe(1);
            publishers.State.First().Value.ShouldContain(groupGAgent1.GetPrimaryKey().ToString());

            var subscribers = new GrainState<List<GrainId>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscribersStateName,
                groupGAgent1.GetGrainId(),
                subscribers);
            subscribers.State.Count.ShouldBe(0);
        }

        // Act.
        await groupGAgent2.UnregisterAsync(naiveTestGAgent);

        // Assert: Check member's states from GrainStorage.
        {
            var subscriptions = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscriptionsStateName,
                naiveTestGAgent.GetGrainId(),
                subscriptions);
            subscriptions.State.Count.ShouldBe(1);

            var publishers = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.PublishersStateName,
                naiveTestGAgent.GetGrainId(),
                publishers);
            publishers.State.Count.ShouldBe(2);
        }

        // Assert: Check groupGAgent2's states from GrainStorage.
        {
            var publishers = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.PublishersStateName,
                groupGAgent2.GetGrainId(),
                publishers);
            publishers.State.Count.ShouldBe(1);
            publishers.State.First().Value.ShouldContain(groupGAgent2.GetPrimaryKey().ToString());

            var subscribers = new GrainState<List<GrainId>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscribersStateName,
                groupGAgent2.GetGrainId(),
                subscribers);
            subscribers.State.Count.ShouldBe(0);
        }

        // Act.
        await groupGAgent3.UnregisterAsync(naiveTestGAgent);

        // Assert: Check member's states from GrainStorage.
        {
            var subscriptions = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscriptionsStateName,
                naiveTestGAgent.GetGrainId(),
                subscriptions);
            subscriptions.State.Count.ShouldBe(0);

            var publishers = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.PublishersStateName,
                naiveTestGAgent.GetGrainId(),
                publishers);
            publishers.State.Count.ShouldBe(1);
        }

        // Assert: Check groupGAgent3's states from GrainStorage.
        {
            var publishers = new GrainState<Dictionary<Guid, string>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.PublishersStateName,
                groupGAgent3.GetGrainId(),
                publishers);
            publishers.State.Count.ShouldBe(1);
            publishers.State.First().Value.ShouldContain(groupGAgent3.GetPrimaryKey().ToString());

            var subscribers = new GrainState<List<GrainId>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscribersStateName,
                groupGAgent3.GetGrainId(),
                subscribers);
            subscribers.State.Count.ShouldBe(0);
        }
    }
}