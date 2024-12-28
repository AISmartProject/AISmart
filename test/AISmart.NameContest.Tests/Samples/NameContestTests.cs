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
using Newtonsoft.Json;
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
                        Bio =  JsonConvert.SerializeObject(new
                        {
                            Description = "James is a renowned NBA superstar known for his exceptional skills on the basketball court, his leadership abilities, and his contributions to the game. With a career spanning over multiple years, he has won numerous awards, including MVP titles and championship rings. Off the court, James is admired for his philanthropy, community involvement, and dedication to inspiring the next generation of athletes."
                        }),
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
        
        [Fact]
        public async Task InitNetworks_Test()
        {
            ContestAgentsDto contestAgentsDto = new ContestAgentsDto()
            {
                ContestantAgentList= new List<ContestantAgent>()
                {
                    new ContestantAgent()
                    {
                        Name = "james",
                        Bio =  JsonConvert.SerializeObject(new
                        {
                            Description = "James is a renowned NBA superstar known for his exceptional skills on the basketball court, his leadership abilities, and his contributions to the game. With a career spanning over multiple years, he has won numerous awards, including MVP titles and championship rings. Off the court, James is admired for his philanthropy, community involvement, and dedication to inspiring the next generation of athletes."
                        }),
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


            NetworksDto networksDto = new NetworksDto()
            {
                Networks = new List<Network>()
                {
                    new Network()
                    {
                        ConstentList = agentResponse.ContestantAgentList.Select(agent => agent.AgentId).ToList(),
                        JudgeList = agentResponse.JudgeAgentList.Select(agent => agent.AgentId).ToList(),
                        ScoreList = agentResponse.JudgeAgentList.Select(agent => agent.AgentId).ToList(),
                        HostList = agentResponse.ContestantAgentList.Select(agent => agent.AgentId).ToList(),
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
                ContestantAgentList= new List<ContestantAgent>()
                {
                    new ContestantAgent()
                    {
                        Name = "james",
                        Bio =  JsonConvert.SerializeObject(new
                        {
                            Description = "James is a renowned NBA superstar known for his exceptional skills on the basketball court, his leadership abilities, and his contributions to the game. With a career spanning over multiple years, he has won numerous awards, including MVP titles and championship rings. Off the court, James is admired for his philanthropy, community involvement, and dedication to inspiring the next generation of athletes."
                        }),
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


            NetworksDto networksDto = new NetworksDto()
            {
                Networks = new List<Network>()
                {
                    new Network()
                    {
                        ConstentList = agentResponse.ContestantAgentList.Select(agent => agent.AgentId).ToList(),
                        JudgeList = agentResponse.JudgeAgentList.Select(agent => agent.AgentId).ToList(),
                        ScoreList = agentResponse.JudgeAgentList.Select(agent => agent.AgentId).ToList(),
                        HostList = agentResponse.ContestantAgentList.Select(agent => agent.AgentId).ToList(),
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

            await Task.Delay(1000 * 10);

        }

    }
}