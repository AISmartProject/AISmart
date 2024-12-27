using System;
using System.Collections.Generic;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using Orleans;

namespace AISmart.Agent;
[GenerateSerializer]
public class NamingContestGAgentState : StateBase
{
    
    [Id(0)] public Dictionary<string, EventBase> ReceiveMessage { get; set; } = new Dictionary<string, EventBase>();


    public void Apply(EventBase eventBase)
    {
        ReceiveMessage["1"] = eventBase;
    }

}