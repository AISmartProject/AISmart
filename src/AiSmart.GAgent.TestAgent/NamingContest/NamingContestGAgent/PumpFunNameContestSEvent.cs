using AISmart.Agents;
using Orleans;

namespace AISmart.Agent.GEvents;



[GenerateSerializer]
public abstract class PumpFunNameContestSEvent:Agents.GEventBase
{
   
}


[GenerateSerializer]
public class IniNetWorkMessagePumpFunSEvent : PumpFunNameContestSEvent
{
    [Id(0)] public List<string> CreativeAgentIdList { get; set; } 
    
    [Id(1)] public List<string> JudgeAgentIdList { get; set; } 
    
    [Id(2)] public List<string> ScoreAgentIdList { get; set; } 
    
    [Id(3)] public List<string> HostAgentIdList { get; set; } 
    
    [Id(4)] public string CallBackUrl { get; set; }
    
    [Id(5)] public string Name { get; set; }
    
    [Id(6)] public string Round { get; set; }
    
    [Id(7)] public Guid GroupId { get; set; }
    [Id(8)] public String MostCharmingBackUrl { get; set; }
    [Id(9)] public Guid MostCharmingGroupId { get; set; }
}