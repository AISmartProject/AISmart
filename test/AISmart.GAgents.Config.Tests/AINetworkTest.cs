using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.Agents.Developer;
using AISmart.Agents.Investment;
using AISmart.Agents.X.Events;
using AISmart.AgentsNetwork;
using AISmart.Application.Grains;
using AISmart.Options;
using AISmart.Sender;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Orleans;
using Orleans.TestingHost.Utils;
using Shouldly;
using Xunit;

namespace AISmart
{
    public sealed class AINetworkTest : AISmartAgentsConfigTestBase, IAsyncLifetime
    {
        private readonly IGrainFactory _grainFactory;
        
        private readonly IAgentNetworkManager _agentNetworkManager;
        private IPublishingGAgent? _publishingGAgent;
        private readonly AgentNetworkConfigOptions _agentNetworkConfigOptions;


        public AINetworkTest()
        {
            _grainFactory = GetRequiredService<IGrainFactory>();
            _agentNetworkConfigOptions = GetRequiredService<IOptionsSnapshot<AgentNetworkConfigOptions>>().Value;
            _agentNetworkManager = GetRequiredService<IAgentNetworkManager>();
        }

        public Task InitializeAsync()
        {
            _publishingGAgent = _grainFactory.GetGrain<IPublishingGAgent>(Guid.NewGuid());
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            // Clean up resources if needed
            return Task.CompletedTask;
        }
        
        private static AgentNetworkConfigOptions LoadConfiguration(string jsonFilePath)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(jsonFilePath, optional: false, reloadOnChange: false);

            IConfiguration configuration = builder.Build();

            var options = new AgentNetworkConfigOptions();
            configuration.GetSection("NetWork").Bind(options);
            return options;
        }

        [Fact]
        public void ConfigurationFromFileTest()
        {
            var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(),"ConfigFile", "appSettingsConfigTest.json");

            File.Exists(jsonFilePath).ShouldBeTrue($"Configuration file {jsonFilePath} not found.");
            
            var configOptions = LoadConfiguration(jsonFilePath);
            
            configOptions.ShouldNotBeNull();
            configOptions.ContractsList.Count.ShouldBeGreaterThan(0);
            configOptions.Groups.Count.ShouldBeGreaterThan(0);
        }
        
        [Fact]
        public async Task Create_Single_DevelopGAgent_From_Config_SUCCESS_TEST()
        {
            var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(),"ConfigFile", "singleDevelopGAgent.json");

            File.Exists(jsonFilePath).ShouldBeTrue($"Configuration file {jsonFilePath} not found.");
            
            
            var configOptions = LoadConfiguration(jsonFilePath);
            
            configOptions.ShouldNotBeNull();
            configOptions.ContractsList.Count.ShouldBe(1);
            configOptions.ContractsList.FirstOrDefault()!.GrainType!.ShouldBe("AISmart.Application.Grains.Agents.Developer.DeveloperGAgent");
            
            
            await _agentNetworkManager.InitializeAgentsAsync(configOptions);
            var agentDictionary = _agentNetworkManager.GetAgentInstances();
            
            agentDictionary.Count.ShouldBe(1);
            agentDictionary.FirstOrDefault().Key.ShouldBe(configOptions.ContractsList.FirstOrDefault()!.Name);
            agentDictionary.FirstOrDefault().Value!.GetType().FullName.ShouldBe("OrleansCodeGen.AISmart.Agents.Proxy_IGAgent");
        }
        
        [Fact]
        public async Task Create_Single_Group_From_Config_SUCCESS_TEST()
        {
            var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(),"ConfigFile", "singleGroupGAgent.json");

            File.Exists(jsonFilePath).ShouldBeTrue($"Configuration file {jsonFilePath} not found.");
            
            
            var configOptions = LoadConfiguration(jsonFilePath);
            
            configOptions.ShouldNotBeNull();
            configOptions.ContractsList.Count.ShouldBe(3);
            configOptions.ContractsList.FirstOrDefault()!.GrainType!.ShouldBe("AISmart.Application.Grains.Agents.Group.GroupGAgent");
            configOptions.ContractsList[1].GrainType!.ShouldBe("AISmart.Application.Grains.Agents.MarketLeader.MarketLeaderGAgent");
            configOptions.ContractsList[2].GrainType!.ShouldBe("AISmart.Application.Grains.Agents.Developer.DeveloperGAgent");
            
            
            await _agentNetworkManager.InitializeAgentsAsync(configOptions);
            var agentDictionary = _agentNetworkManager.GetAgentInstances();
            
            agentDictionary.Count.ShouldBe(2);
            agentDictionary.FirstOrDefault().Key.ShouldBe(configOptions.ContractsList[1].Name);
            agentDictionary.ElementAtOrDefault(1).Key.ShouldBe(configOptions.ContractsList[2].Name);

            agentDictionary.FirstOrDefault().Value!.GetType().FullName.ShouldBe("OrleansCodeGen.AISmart.Agents.Proxy_IGAgent");
            agentDictionary.ElementAtOrDefault(1).Value!.GetType().FullName.ShouldBe("OrleansCodeGen.AISmart.Agents.Proxy_IGAgent");
        }
        

        [Fact]
        public async Task Agent_NetWork_Test()
        {
            await _agentNetworkManager.InitializeAgentsAsync(_agentNetworkConfigOptions);

            var agentDictionary = _agentNetworkManager.GetAgentInstances();
            agentDictionary.Count.ShouldBe(7);
            
            var groupAgents = _agentNetworkManager.GetGroupAgents();
            var gAgents = groupAgents as IGAgent[] ?? groupAgents.ToArray();
            gAgents.Length.ShouldBeGreaterThan(0);
            
            var groupStateGAgent = gAgents.FirstOrDefault()!;
            groupStateGAgent.GetType().ShouldBe(typeof(IGAgent));

            _publishingGAgent = _grainFactory.GetGrain<IPublishingGAgent>(Guid.NewGuid());
            await _publishingGAgent.PublishTo(groupStateGAgent);

            const string content = "BTC REACHED 100k BOOHOO!";
            var xThreadCreatedEvent = new XThreadCreatedEvent
            {
                Id = "mock_x_thread_id",
                Content = content
            };
            await _publishingGAgent.PublishEventAsync(xThreadCreatedEvent);

            var investmentAgent = GrainTracker.InvestmentAgents.First();
            var investmentAgentState = await investmentAgent.GetStateAsync();
            
            var developerAgent = GrainTracker.DeveloperAgents.First();
            var developerAgentState = await developerAgent.GetStateAsync();

            await TestingUtils.WaitUntilAsync(_ => CheckState(developerAgentState), TimeSpan.FromSeconds(20));
            await TestingUtils.WaitUntilAsync(_ => CheckState(investmentAgentState), TimeSpan.FromSeconds(20));
            
            developerAgentState.Content.Count.ShouldBe(1);
            investmentAgentState.Content.Count.ShouldBe(1);
        }

        private static Task<bool> CheckState(InvestmentAgentState state)
        {
            return Task.FromResult(!CollectionUtilities.IsNullOrEmpty(state.Content));
        }
        
        private static Task<bool> CheckState(DeveloperAgentState state)
        {
            return Task.FromResult(!CollectionUtilities.IsNullOrEmpty(state.Content));
        }
    }
}