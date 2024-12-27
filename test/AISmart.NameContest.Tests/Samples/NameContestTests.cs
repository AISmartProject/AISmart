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
using AISmart.Service;
using Microsoft.IdentityModel.Tokens;
using Orleans;
using Orleans.TestingHost.Utils;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AISmart.Samples
{
    public class NameContestTests : AISmartNameContestTestBase
    {


        private INamingContestService _namingContestService;
       

        public NameContestTests(ITestOutputHelper output)
        {
            _namingContestService = GetRequiredService<INamingContestService>();
            
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
            ContestAgentsDto contestAgentsDto = new ContestAgentsDto()
            {
                ContestantAgentList= new List<ContestantAgent>()
                {
                    new ContestantAgent()
                    {
                        Name = "james",
                    },
                    new ContestantAgent()
                    {
                        Name = "kob",
                    },
                },
                JudgeAgentList= new List<JudgeAgent>()
                {
                    new JudgeAgent()
                    {
                        Name = "james",
                    },
                    new JudgeAgent()
                    {
                        Name = "kob",
                    },
                },
                HostAgentList = new List<HostAgent>()
                {
                    
                }

            };
            AgentResponse agentResponse = await _namingContestService.InitAgentsAsync(contestAgentsDto);
            
            agentResponse.ContestantAgentList.Count.ShouldBe(2);
            agentResponse.ContestantAgentList.FirstOrDefault()!.Name.ShouldBe("james");
            agentResponse.ContestantAgentList[1].Name.ShouldBe("kob");
            
            
            agentResponse.JudgeAgentList.Count.ShouldBe(2);
            agentResponse.JudgeAgentList.FirstOrDefault()!.Name.ShouldBe("james");
            agentResponse.JudgeAgentList[1].Name.ShouldBe("kob");

        }

    }
}