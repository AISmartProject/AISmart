using System;
using System.Collections.Generic;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using Orleans;

namespace AiSmart.GAgent.TestAgent.NamingContest.ManagerAgent;

[GenerateSerializer]
public class ManagerAgentState: StateBase
{
    [Id(0)] public List<string> CreativeAgentIdList { get; set; } = new List<string>();
    
    [Id(1)] public List<string> JudgeAgentIdList { get; set; } = new List<string>();
    
    [Id(2)] public List<string> HostAgentIdList { get; set; } = new List<string>();
    
    
    [Id(3)] public Dictionary<string, IniNetWorkMessageSEvent> NetworkDictionary { get; set; } = new Dictionary<string, IniNetWorkMessageSEvent>();

    
    

    public void Apply(InitAgentMessageSEvent initAgentMessageSEvent)
    {
        CreativeAgentIdList.AddRange(initAgentMessageSEvent.CreativeAgentIdList);
        JudgeAgentIdList.AddRange(initAgentMessageSEvent.JudgeAgentIdList);
        HostAgentIdList.AddRange(initAgentMessageSEvent.HostAgentIdList);
    }
    
    public void Apply(IniNetWorkMessageSEvent iniNetWorkMessageSEvent)
    {
        NetworkDictionary[iniNetWorkMessageSEvent.GroupAgentId] = iniNetWorkMessageSEvent;
    }
    
    

}

