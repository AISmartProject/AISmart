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
    
    
    [Id(3)] public Dictionary<string, IniNetWorkMessageGEvent> NetworkDictionary { get; set; } = new Dictionary<string, IniNetWorkMessageGEvent>();

    
    

    public void Apply(InitAgentMessageGEvent initAgentMessageGEvent)
    {
        CreativeAgentIdList.AddRange(initAgentMessageGEvent.CreativeAgentIdList);
        JudgeAgentIdList.AddRange(initAgentMessageGEvent.JudgeAgentIdList);
        HostAgentIdList.AddRange(initAgentMessageGEvent.HostAgentIdList);
    }
    
    public void Apply(IniNetWorkMessageGEvent iniNetWorkMessageGEvent)
    {
        NetworkDictionary[iniNetWorkMessageGEvent.GroupAgentId] = iniNetWorkMessageGEvent;
    }
    
    

}

