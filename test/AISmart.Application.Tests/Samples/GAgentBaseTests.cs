using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.Agents.Group;
using AISmart.Application.Grains.Agents.Group;
using AISmart.GAgents.Tests;
using AISmart.GAgents.Tests.TestEvents;
using AISmart.GAgents.Tests.TestGAgents;
using AISmart.Sender;
using Orleans;
using Orleans.Runtime;
using Shouldly;
using Xunit;

namespace AISmart.Samples;

public class GAgentBaseTests : AISmartApplicationTestBase
{
    protected readonly IGrainFactory _grainFactory;

    public GAgentBaseTests()
    {
        _grainFactory = GetRequiredService<IGrainFactory>();

    }
    [Fact]
    public async Task ComplicatedEventHandleTest()
    {
        // Arrange.
        var marketingLeader = _grainFactory.GetGrain<IMarketingLeaderTestGAgent>(Guid.NewGuid());
        var developingLeader = _grainFactory.GetGrain<IDevelopingLeaderTestGAgent>(Guid.NewGuid());

        var developer1 = _grainFactory.GetGrain<IDeveloperTestGAgent>(Guid.NewGuid());
        var developer2 = _grainFactory.GetGrain<IDeveloperTestGAgent>(Guid.NewGuid());
        var developer3 = _grainFactory.GetGrain<IDeveloperTestGAgent>(Guid.NewGuid());
        await developingLeader.RegisterAsync(developer1);
        await developingLeader.RegisterAsync(developer2);
        await developingLeader.RegisterAsync(developer3);

        var investor1 = _grainFactory.GetGrain<IStateGAgent<InvestorTestGAgentState>>(Guid.NewGuid());
        var investor2 = _grainFactory.GetGrain<IStateGAgent<InvestorTestGAgentState>>(Guid.NewGuid());
        await marketingLeader.RegisterAsync(investor1);
        await marketingLeader.RegisterAsync(investor2);

        var groupGAgent = _grainFactory.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
        await groupGAgent.RegisterAsync(marketingLeader);
        await groupGAgent.RegisterAsync(developingLeader);
        var publishingGAgent = _grainFactory.GetGrain<IPublishingGAgent>(Guid.NewGuid());
        await publishingGAgent.RegisterAsync(groupGAgent);

        // Act.
        await publishingGAgent.PublishEventAsync(new NewDemandTestEvent
        {
            Description = "New demand from customer."
        });

        await TestHelper.WaitUntilAsync(_ => CheckState(investor1), TimeSpan.FromSeconds(20));

        var groupState = await groupGAgent.GetStateAsync();
        groupState.RegisteredAgents.ShouldBe(2);

        var investorState = await investor1.GetStateAsync();
        investorState.Content.Count.ShouldBePositive();
    }
    
    private async Task<bool> CheckState(IStateGAgent<InvestorTestGAgentState> investor1)
    {
        var state = await investor1.GetStateAsync();
        return !state.Content.IsNullOrEmpty() && state.Content.Count == 2;
    }
}