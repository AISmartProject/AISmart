using System;
using System.Linq;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.Agents.X.Events;
using AISmart.Application.Grains;
using AISmart.Application.Grains.Agents.Developer;
using AISmart.Application.Grains.Agents.Group;
using AISmart.Application.Grains.Agents.MarketLeader;
using AISmart.Application.Grains.Agents.X;
using AISmart.Sender;
using Microsoft.IdentityModel.Tokens;
using Orleans;
using Orleans.TestingHost.Utils;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AISmart.Samples
{
    public class XAgentTests : AISmartApplicationTestBase, IAsyncLifetime
    {
        private readonly IClusterClient _clusterClient;
        protected readonly IGrainFactory _grainFactory;

        private IStateAgent<XAgentState> _xStateAgent;
        private IStateAgent<MarketLeaderAgentState> _marketLeaderStateAgent;
        private IStateAgent<DeveloperAgentState> _developerStateAgent;
        private IStateAgent<InvestmentAgentState> _investmentStateAgent;
        private IStateAgent<GroupAgentState> _groupStateAgent;
        private IPublishingAgent _publishingAgent;

        public XAgentTests(ITestOutputHelper output)
        {
            _clusterClient = GetRequiredService<IClusterClient>();
            _grainFactory = GetRequiredService<IGrainFactory>();
        }

        public async Task InitializeAsync()
        {
            _xStateAgent = _grainFactory.GetGrain<IStateAgent<XAgentState>>(Guid.NewGuid());
            _marketLeaderStateAgent = _grainFactory.GetGrain<IStateAgent<MarketLeaderAgentState>>(Guid.NewGuid());
            _developerStateAgent = _grainFactory.GetGrain<IStateAgent<DeveloperAgentState>>(Guid.NewGuid());
            _investmentStateAgent = _grainFactory.GetGrain<IStateAgent<InvestmentAgentState>>(Guid.NewGuid());
            _groupStateAgent = _grainFactory.GetGrain<IStateAgent<GroupAgentState>>(Guid.NewGuid());
            
            await _groupStateAgent.Register(_xStateAgent);
            await _groupStateAgent.Register(_marketLeaderStateAgent);
            await _groupStateAgent.Register(_developerStateAgent);
            await _groupStateAgent.Register(_investmentStateAgent);

            _publishingAgent = _clusterClient.GetGrain<IPublishingAgent>(Guid.NewGuid());
            await _publishingAgent.PublishTo(_groupStateAgent);
        }

        public Task DisposeAsync()
        {
            // Clean up resources if needed
            return Task.CompletedTask;
        }

        [Fact]
        public async Task XThreadCreatedEvent_Executed_Test()
        {
            const string content = "BTC REACHED 100k WOOHOOOO!";

            var xThreadCreatedEvent = new XThreadCreatedEvent
            {
                Id = "mock_x_thread_id",
                Content = content
            };

            await _publishingAgent.PublishEventAsync(xThreadCreatedEvent);

            var investmentAgent = GrainTracker.InvestmentAgents.First();
            var investmentAgentState = await investmentAgent.GetStateAsync();
            
            var developerAgent = GrainTracker.DeveloperAgents.First();
            var developerAgentState = await developerAgent.GetStateAsync();

            await TestingUtils.WaitUntilAsync(_ => CheckState(developerAgentState), TimeSpan.FromSeconds(20));
            await TestingUtils.WaitUntilAsync(_ => CheckState(investmentAgentState), TimeSpan.FromSeconds(20));
            
            developerAgentState.Content.Count.ShouldBe(1);
            investmentAgentState.Content.Count.ShouldBe(1);
        }

        private async Task<bool> CheckState(InvestmentAgentState state)
        {
            return !state.Content.IsNullOrEmpty();
        }
        
        private async Task<bool> CheckState(DeveloperAgentState state)
        {
            return !state.Content.IsNullOrEmpty();
        }
    }
}