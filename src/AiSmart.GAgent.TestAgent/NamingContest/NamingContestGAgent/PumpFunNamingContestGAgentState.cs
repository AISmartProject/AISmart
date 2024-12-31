using System;
using System.Collections.Generic;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using Orleans;

namespace AISmart.Agent;
[GenerateSerializer]
public class PumpFunNamingContestGAgentState : StateBase
{
    
    [Id(0)] public Dictionary<string, EventBase> ReceiveMessage { get; set; } = new Dictionary<string, EventBase>();
    
    [Id(1)] public string CallBackUrl { get; set; }
    
    [Id(2)] public string Name { get; set; }
    
    
    
    [Id(3)] public List<string> CreativeAgentIdList { get; set; } 
    
    [Id(4)] public List<string> JudgeAgentIdList { get; set; } 
    
    [Id(5)] public List<string> JudgeScoreAgentIdList { get; set; } 
    
    [Id(6)] public List<string> HostAgentIdList { get; set; } 
    
    [Id(7)] public string Round { get; set; }

    


    public void Apply(IniNetWorkMessagePumpFunGEvent iniNetWorkMessageGEvent)
    {
        CallBackUrl = iniNetWorkMessageGEvent.CallBackUrl;
        Name = iniNetWorkMessageGEvent.Name;
        Round = iniNetWorkMessageGEvent.Round;
        CreativeAgentIdList = iniNetWorkMessageGEvent.CreativeAgentIdList;
        JudgeAgentIdList = iniNetWorkMessageGEvent.JudgeAgentIdList;
        JudgeScoreAgentIdList = iniNetWorkMessageGEvent.ScoreAgentIdList;
        HostAgentIdList = iniNetWorkMessageGEvent.HostAgentIdList;
    }

}