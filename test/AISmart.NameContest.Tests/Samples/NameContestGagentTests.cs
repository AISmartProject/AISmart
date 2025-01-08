using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AISmart.Agent;
using AISmart.Agents;
using AISmart.Agents.Developer;
using AISmart.Agents.Group;
using AISmart.Agents.Investment;
using AISmart.Agents.MarketLeader;
using AISmart.Agents.X;
using AISmart.Agents.X.Events;
using AiSmart.GAgent.TestAgent.NamingContest.Common;
using AiSmart.GAgent.TestAgent.NamingContest.TrafficAgent;
using AISmart.Grains;
using AISmart.Provider;
using AISmart.Sender;
using AISmart.Service;
using Microsoft.IdentityModel.Tokens;
using Orleans;
using Orleans.TestingHost.Utils;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AISmart.Samples
{
    public sealed class NameContestGagentTests : AISmartNameContestTestBase
    {
        private readonly IClusterClient _clusterClient;

        private INamingContestService _namingContestService;
        private readonly IPumpFunNamingContestGAgent _pumpFunNamingContestGAgent;
        private readonly INamingContestGrain _namingContestGrain;


        public NameContestGagentTests(ITestOutputHelper output)
        {
            _namingContestService = GetRequiredService<INamingContestService>();
            _clusterClient = GetRequiredService<IClusterClient>();
            _pumpFunNamingContestGAgent = _clusterClient.GetGrain<IPumpFunNamingContestGAgent>(Guid.NewGuid());
            _namingContestGrain = _clusterClient.GetGrain<INamingContestGrain>("NamingContestGrain");
        }

        public async Task InitializeAsync()
        {
        }

        public Task DisposeAsync()
        {
            // Clean up resources if needed
            return Task.CompletedTask;
        }

        [Fact]
        public async Task InitAgents_Test()
        {

            var namingLogEvent = new NamingLogEvent(NamingContestStepEnum.Complete, Guid.Empty);

            await _namingContestGrain.SendMessageAsync(Guid.NewGuid(),namingLogEvent as NamingLogEvent,"https://xxx.com");
        }
    }
}