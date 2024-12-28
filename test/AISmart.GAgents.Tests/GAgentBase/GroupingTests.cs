using AISmart.Application.Grains.Agents.Group;
using AISmart.GAgent.Core;
using AISmart.GAgents.Tests.TestEvents;
using AISmart.GAgents.Tests.TestGAgents;
using AISmart.Sender;
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

        // Assert: Check group's states from GrainStorage.
        var subscribers = new GrainState<List<GrainId>>();
        await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscribersStateName,
            groupGAgent.GetGrainId(),
            subscribers);
        subscribers.State.Count.ShouldBe(1);
        subscribers.State.First().ShouldBe(naiveTestGAgent.GetGrainId());
    }

    [Fact(DisplayName = "Each gAgent's states should be saved correctly after unregister.")]
    public async Task UnregisterTest()
    {
        // Arrange.
        var naiveTestGAgent = await Silo.CreateGrainAsync<NaiveTestGAgent>(Guid.NewGuid());
        var groupGAgent = await CreateGroupGAgentAsync(naiveTestGAgent);

        // Act.
        await groupGAgent.UnregisterAsync(naiveTestGAgent);

        // Assert: Check group's states from GrainStorage.
        var subscribers = new GrainState<List<GrainId>>();
        await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscribersStateName,
            groupGAgent.GetGrainId(),
            subscribers);
        subscribers.State.Count.ShouldBe(0);
    }

    [Fact(DisplayName = "Multiple gAgents should be registered correctly to one group.")]
    public async Task OneGroupRegisterMultipleGAgentsTest()
    {
        // Arrange & Act.
        var naiveTestGAgent1 = await Silo.CreateGrainAsync<NaiveTestGAgent>(Guid.NewGuid());
        var naiveTestGAgent2 = await Silo.CreateGrainAsync<NaiveTestGAgent>(Guid.NewGuid());
        var naiveTestGAgent3 = await Silo.CreateGrainAsync<NaiveTestGAgent>(Guid.NewGuid());
        var groupGAgent = await CreateGroupGAgentAsync(naiveTestGAgent1, naiveTestGAgent2, naiveTestGAgent3);

        // Assert: Check group's states from GrainStorage.
        var subscribers = new GrainState<List<GrainId>>();
        await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscribersStateName,
            groupGAgent.GetGrainId(),
            subscribers);
        subscribers.State.Count.ShouldBe(3);
    }

    [Fact(DisplayName = "One gAgent should be registered correctly to multiple group.")]
    public async Task MultipleGroupRegisterOneGAgentTest()
    {
        // Arrange & Act.
        var naiveTestGAgent = await Silo.CreateGrainAsync<NaiveTestGAgent>(Guid.NewGuid());
        var groupGAgent1 = await CreateGroupGAgentAsync(naiveTestGAgent);
        var groupGAgent2 = await CreateGroupGAgentAsync(naiveTestGAgent);
        var groupGAgent3 = await CreateGroupGAgentAsync(naiveTestGAgent);

        // Assert: Check each group's states from GrainStorage.
        foreach (var groupGAgent in new List<GroupGAgent> { groupGAgent1, groupGAgent2, groupGAgent3 })
        {
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

        // Assert: Check groupGAgent1's states from GrainStorage.
        {
            var subscribers = new GrainState<List<GrainId>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscribersStateName,
                groupGAgent1.GetGrainId(),
                subscribers);
            subscribers.State.Count.ShouldBe(0);
        }

        // Act.
        await groupGAgent2.UnregisterAsync(naiveTestGAgent);

        // Assert: Check groupGAgent2's states from GrainStorage.
        {
            var subscribers = new GrainState<List<GrainId>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscribersStateName,
                groupGAgent2.GetGrainId(),
                subscribers);
            subscribers.State.Count.ShouldBe(0);
        }

        // Act.
        await groupGAgent3.UnregisterAsync(naiveTestGAgent);

        // Assert: Check groupGAgent3's states from GrainStorage.
        {
            var subscribers = new GrainState<List<GrainId>>();
            await Silo.TestGrainStorage.ReadStateAsync(AISmartGAgentConstants.SubscribersStateName,
                groupGAgent3.GetGrainId(),
                subscribers);
            subscribers.State.Count.ShouldBe(0);
        }
    }

    [Fact]
    public async Task DAGTest()
    {
        var marketingLeader = await Silo.CreateGrainAsync<MarketingLeaderTestGAgent>(Guid.NewGuid());
        var developingLeader = await Silo.CreateGrainAsync<DevelopingLeaderTestGAgent>(Guid.NewGuid());

        var developer1 = await Silo.CreateGrainAsync<DeveloperTestGAgent>(Guid.NewGuid());
        var developer2 = await Silo.CreateGrainAsync<DeveloperTestGAgent>(Guid.NewGuid());
        var developer3 = await Silo.CreateGrainAsync<DeveloperTestGAgent>(Guid.NewGuid());
        await developingLeader.RegisterAsync(developer1);
        await developingLeader.RegisterAsync(developer2);
        await developingLeader.RegisterAsync(developer3);

        var investor1 = await Silo.CreateGrainAsync<InvestorTestGAgent>(Guid.NewGuid());
        var investor2 = await Silo.CreateGrainAsync<InvestorTestGAgent>(Guid.NewGuid());
        await marketingLeader.RegisterAsync(investor1);
        await marketingLeader.RegisterAsync(investor2);

        var groupGAgent = await CreateGroupGAgentAsync(marketingLeader, developingLeader);
        var publishingGAgent = await CreatePublishingGAgentAsync(groupGAgent);

        AddProbesByGrainId(marketingLeader, developingLeader, developer1, developer2, developer3, investor1, investor2);

        await publishingGAgent.PublishEventAsync(new NewDemandTestEvent
        {
            Description = "New demand from customer."
        });
        
        var investorState = await investor1.GetStateAsync();
        investorState.Content.Count.ShouldBe(4);
        var newLineCount = investorState.Content.Last().Count(c => c == '\n');
        newLineCount.ShouldBe(2);
    }
}