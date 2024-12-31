using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.Agents.Developer;
using AISmart.Agents.Group;
using AISmart.Agents.Investment;
using AISmart.Agents.MarketLeader;
using AISmart.Agents.X;
using AISmart.Agents.X.Events;
using AISmart.Sender;
using Microsoft.IdentityModel.Tokens;
using Orleans;
using Orleans.TestingHost.Utils;
using Xunit;
using Xunit.Abstractions;

namespace AISmart.Samples
{
    public class XgAgentTests : AISmartApplicationTestBase, IAsyncLifetime
    {
        private readonly IClusterClient _clusterClient;
        protected readonly IGrainFactory _grainFactory;

        private IStateGAgent<XAgentState> _xStateGAgent;
        private IStateGAgent<MarketLeaderAgentState> _marketLeaderStateGAgent;
        private IStateGAgent<DeveloperAgentState> _developerStateGAgent;
        private IStateGAgent<InvestmentAgentState> _investmentStateGAgent;
        private IStateGAgent<GroupAgentState> _groupStateGAgent;
        private IPublishingGAgent _publishingGAgent;

        public XgAgentTests(ITestOutputHelper output)
        {
            _clusterClient = GetRequiredService<IClusterClient>();
            _grainFactory = GetRequiredService<IGrainFactory>();
        }

        public async Task InitializeAsync()
        {
            _xStateGAgent = _grainFactory.GetGrain<IStateGAgent<XAgentState>>(Guid.NewGuid());
            _marketLeaderStateGAgent = _grainFactory.GetGrain<IStateGAgent<MarketLeaderAgentState>>(Guid.NewGuid());
            _developerStateGAgent = _grainFactory.GetGrain<IStateGAgent<DeveloperAgentState>>(Guid.NewGuid());
            _investmentStateGAgent = _grainFactory.GetGrain<IStateGAgent<InvestmentAgentState>>(Guid.NewGuid());
            _groupStateGAgent = _grainFactory.GetGrain<IStateGAgent<GroupAgentState>>(Guid.NewGuid());
            
            await _groupStateGAgent.RegisterAsync(_xStateGAgent);
            await _groupStateGAgent.RegisterAsync(_marketLeaderStateGAgent);
            await _groupStateGAgent.RegisterAsync(_developerStateGAgent);
            await _groupStateGAgent.RegisterAsync(_investmentStateGAgent);

            _publishingGAgent = _clusterClient.GetGrain<IPublishingGAgent>(Guid.NewGuid());
            await _publishingGAgent.RegisterAsync(_groupStateGAgent);
        }

        public Task DisposeAsync()
        {
            // Clean up resources if needed
            return Task.CompletedTask;
        }

        // TODO: Re-impl this test case.
        // [Fact]
        // public async Task XThreadCreatedEvent_Executed_Test()
        // {
        //     const string content = "BTC REACHED 100k WOOHOOOO!";
        //
        //     var xThreadCreatedEvent = new XThreadCreatedEvent
        //     {
        //         Id = "mock_x_thread_id",
        //         Content = content
        //     };
        //
        //     await _publishingGAgent.PublishEventAsync(xThreadCreatedEvent);
        //
        //     var investmentAgent = GrainTracker.InvestmentAgents.First();
        //     var investmentAgentState = await investmentAgent.GetStateAsync();
        //     
        //     var developerAgent = GrainTracker.DeveloperAgents.First();
        //     var developerAgentState = await developerAgent.GetStateAsync();
        //
        //     await TestingUtils.WaitUntilAsync(_ => CheckState(developerAgentState), TimeSpan.FromSeconds(20));
        //     await TestingUtils.WaitUntilAsync(_ => CheckState(investmentAgentState), TimeSpan.FromSeconds(20));
        //     
        //     developerAgentState.Content.Count.ShouldBe(1);
        //     investmentAgentState.Content.Count.ShouldBe(1);
        // }
        //
        // private async Task<bool> CheckState(InvestmentAgentState state)
        // {
        //     return !state.Content.IsNullOrEmpty();
        // }
        //
        // private async Task<bool> CheckState(DeveloperAgentState state)
        // {
        //     return !state.Content.IsNullOrEmpty();
        // }
        // [Fact]
        // public async Task XThreadCreatedEvent_Executed_Test()
        // {
        //     const string content = "BTC REACHED 100k WOOHOOOO!";
        //
        //     var xThreadCreatedEvent = new XThreadCreatedEvent
        //     {
        //         Id = "mock_x_thread_id",
        //         Content = content
        //     };
        //
        //     await _publishingGAgent.PublishEventAsync(xThreadCreatedEvent);
        //
        //     var investmentAgent = GrainTracker.InvestmentAgents.First();
        //     var investmentAgentState = await investmentAgent.GetStateAsync();
        //     
        //     var developerAgent = GrainTracker.DeveloperAgents.First();
        //     var developerAgentState = await developerAgent.GetStateAsync();
        //
        //     await TestingUtils.WaitUntilAsync(_ => CheckState(developerAgentState), TimeSpan.FromSeconds(20));
        //     await TestingUtils.WaitUntilAsync(_ => CheckState(investmentAgentState), TimeSpan.FromSeconds(20));
        //     
        //     developerAgentState.Content.Count.ShouldBe(1);
        //     investmentAgentState.Content.Count.ShouldBe(1);
        // }

        private async Task<bool> CheckState(InvestmentAgentState state)
        {
            return !CollectionUtilities.IsNullOrEmpty(state.Content);
        }
        
        private async Task<bool> CheckState(DeveloperAgentState state)
        {
            return !CollectionUtilities.IsNullOrEmpty(state.Content);
        }
    }
}