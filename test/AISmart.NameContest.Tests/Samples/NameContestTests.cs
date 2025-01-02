using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using AISmart.Agents;
using AISmart.Agents.Developer;
using AISmart.Agents.Group;
using AISmart.Agents.Investment;
using AISmart.Agents.MarketLeader;
using AISmart.Agents.X;
using AISmart.Agents.X.Events;
using AiSmart.GAgent.TestAgent.NamingContest.CreativeAgent;
using AISmart.Sender;
using AISmart.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Orleans;
using Orleans.TestingHost.Utils;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AISmart.Samples
{
    public sealed class NameContestTests : AISmartNameContestTestBase
    {
        private readonly INamingContestService _namingContestService;
        private readonly IClusterClient _clusterClient;



        public NameContestTests(ITestOutputHelper output)
        {
            _namingContestService = GetRequiredService<INamingContestService>();
            _clusterClient = GetRequiredService<IClusterClient>();

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
                ContestantAgentList = new List<ContestantAgent>()
                {
                    new ContestantAgent()
                    {
                        Name = "james",
                        Bio = JsonSerializer.Serialize(new
                        {
                            Description =
                                "James is a renowned NBA superstar known for his exceptional skills on the basketball court, his leadership abilities, and his contributions to the game. With a career spanning over multiple years, he has won numerous awards, including MVP titles and championship rings. Off the court, James is admired for his philanthropy, community involvement, and dedication to inspiring the next generation of athletes."
                        }),
                    },
                    new ContestantAgent()
                    {
                        Name = "kob",
                    },
                },
                JudgeAgentList = new List<JudgeAgent>()
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
            AiSmartInitResponse agentResponse = await _namingContestService.InitAgentsAsync(contestAgentsDto);

            agentResponse.Details.Count.ShouldBe(4);
            agentResponse.Details.FirstOrDefault()!.AgentName.ShouldBe("james");
            agentResponse.Details[1].AgentName.ShouldBe("kob");


            agentResponse.Details.Count.ShouldBe(2);
            agentResponse.Details.FirstOrDefault()!.AgentName.ShouldBe("james");
            agentResponse.Details[1].AgentName.ShouldBe("kob");
        }

        [Fact]
        public async Task InitNetworks_Test()
        {
            ContestAgentsDto contestAgentsDto = new ContestAgentsDto()
            {
                ContestantAgentList = new List<ContestantAgent>()
                {
                    new ContestantAgent()
                    {
                        Name = "james",
                        Bio = JsonSerializer.Serialize(new
                        {
                            Description =
                                "James is a renowned NBA superstar known for his exceptional skills on the basketball court, his leadership abilities, and his contributions to the game. With a career spanning over multiple years, he has won numerous awards, including MVP titles and championship rings. Off the court, James is admired for his philanthropy, community involvement, and dedication to inspiring the next generation of athletes."
                        }),
                    },
                    new ContestantAgent()
                    {
                        Name = "kob",
                    },
                },
                JudgeAgentList = new List<JudgeAgent>()
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
            AiSmartInitResponse agentResponse = await _namingContestService.InitAgentsAsync(contestAgentsDto);

            agentResponse.Details.Count.ShouldBe(4);
            agentResponse.Details.FirstOrDefault()!.AgentName.ShouldBe("james");
            agentResponse.Details[1].AgentName.ShouldBe("kob");


            agentResponse.Details.Count.ShouldBe(2);
            agentResponse.Details.FirstOrDefault()!.AgentName.ShouldBe("james");
            agentResponse.Details[1].AgentName.ShouldBe("kob");


            NetworksDto networksDto = new NetworksDto()
            {
                Networks = new List<Network>()
                {
                    new Network()
                    {
                        // ConstentList = agentResponse.ContestantAgentList.Select(agent => agent.AgentId).ToList(),
                        // JudgeList = agentResponse.JudgeAgentList.Select(agent => agent.AgentId).ToList(),
                        // ScoreList = agentResponse.JudgeAgentList.Select(agent => agent.AgentId).ToList(),
                        // HostList = agentResponse.ContestantAgentList.Select(agent => agent.AgentId).ToList(),
                        Name = "FirstRound-1",
                        CallbackAddress = "https://xxxx.com"
                    }
                }
            };
            GroupResponse groupResponse = await _namingContestService.InitNetworksAsync(networksDto);

            groupResponse.GroupDetails.Count.ShouldBe(1);
            groupResponse.GroupDetails.FirstOrDefault()!.Name.ShouldBe("FirstRound-1");
            groupResponse.GroupDetails.FirstOrDefault()!.GroupId.ShouldNotBeNull();
        }

        [Fact]
        public async Task Start_Group_Test()
        {
            ContestAgentsDto contestAgentsDto = new ContestAgentsDto()
            {
                ContestantAgentList = new List<ContestantAgent>()
                {
                    new ContestantAgent()
                    {
                        Name = "james",
                        Bio = JsonSerializer.Serialize(new
                        {
                            Description =
                                "James is a renowned NBA superstar known for his exceptional skills on the basketball court, his leadership abilities, and his contributions to the game. With a career spanning over multiple years, he has won numerous awards, including MVP titles and championship rings. Off the court, James is admired for his philanthropy, community involvement, and dedication to inspiring the next generation of athletes."
                        }),
                    },
                    new ContestantAgent()
                    {
                        Name = "kob",
                    },
                },
                JudgeAgentList = new List<JudgeAgent>()
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
            AiSmartInitResponse agentResponse = await _namingContestService.InitAgentsAsync(contestAgentsDto);

            agentResponse.Details.Count.ShouldBe(4);
            agentResponse.Details.FirstOrDefault()!.AgentName.ShouldBe("james");
            agentResponse.Details[1].AgentName.ShouldBe("kob");


            agentResponse.Details.Count.ShouldBe(2);
            agentResponse.Details.FirstOrDefault()!.AgentName.ShouldBe("james");
            agentResponse.Details[1].AgentName.ShouldBe("kob");


            NetworksDto networksDto = new NetworksDto()
            {
                Networks = new List<Network>()
                {
                    new Network()
                    {
                        // ConstentList = agentResponse.ContestantAgentList.Select(agent => agent.AgentId).ToList(),
                        // JudgeList = agentResponse.JudgeAgentList.Select(agent => agent.AgentId).ToList(),
                        // ScoreList = agentResponse.JudgeAgentList.Select(agent => agent.AgentId).ToList(),
                        // HostList = agentResponse.ContestantAgentList.Select(agent => agent.AgentId).ToList(),
                        Name = "FirstRound-1",
                        CallbackAddress = "https://xxxx.com"
                    }
                }
            };
            GroupResponse groupResponse = await _namingContestService.InitNetworksAsync(networksDto);

            groupResponse.GroupDetails.Count.ShouldBe(1);
            groupResponse.GroupDetails.FirstOrDefault()!.Name.ShouldBe("FirstRound-1");
            groupResponse.GroupDetails.FirstOrDefault()!.GroupId.ShouldNotBeNull();

            GroupDto groupDto = new GroupDto()
            {
                GroupIdList = new List<string>()
                {
                    groupResponse.GroupDetails.FirstOrDefault()!.GroupId
                }
            };

            await _namingContestService.StartGroupAsync(groupDto);
        }


        [Fact]
        public async Task Init_Multi_Agents_With_Bio_Test()
        {
            var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Samples", "NA101-200.json");

            var contestantAgentList =  LoadConfiguration(jsonFilePath);

            ContestAgentsDto contestAgentsDto = new ContestAgentsDto()
            {
                ContestantAgentList = contestantAgentList,
                JudgeAgentList = new List<JudgeAgent>()
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
            AiSmartInitResponse agentResponse = await _namingContestService.InitAgentsAsync(contestAgentsDto);

            agentResponse.Details.Count.ShouldBe(4);
            agentResponse.Details.FirstOrDefault()!.AgentName.ShouldBe("james");
            agentResponse.Details[1].AgentName.ShouldBe("kob");
            
            
            var agentId = agentResponse.Details.FirstOrDefault()!.AgentId;
            var creativeGAgent = _clusterClient.GetGrain<ICreativeGAgent>(Guid.Parse(agentId));
            var state = creativeGAgent.GetAgentState();
            state.Result.AgentResponsibility.ShouldBe(contestantAgentList.FirstOrDefault()!.Bio);
            state.Result.AgentName.ShouldBe(contestantAgentList.FirstOrDefault()!.Name);

            agentResponse.Details[1].AgentName.ShouldBe("kob");
        }


        private static List<ContestantAgent>? LoadConfiguration(string jsonFilePath)
        {
            // Read the entire file into a string
            string jsonString = File.ReadAllText(jsonFilePath);

            // Assume the JSON array is an array of objects
            // Replace MyClass with the appropriate class for your JSON structure
            List<ContestantAgent>? items = JsonSerializer.Deserialize<List<ContestantAgent>>(jsonString);

            return items;
        }
    }
}