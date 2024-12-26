using System;
using AISmart.Agent.GEvents;
using AISmart.Agents;
using Orleans;

namespace AISmart.Agent;

public class TwitterGAgentState : StateBase
{
    [Id(0)] public Guid Id { get; set; } = Guid.NewGuid();
    [Id(1)] public string UserId { get; set; }
    [Id(2)] public string Token { get; set; }
    [Id(3)] public string TokenSecret { get; set; }
    
    public void Apply(BindTwitterAccountEvent bindTwitterAccountEvent)
    {
        UserId = bindTwitterAccountEvent.UserId;
        Token = bindTwitterAccountEvent.Token;
        TokenSecret = bindTwitterAccountEvent.TokenSecret;
    }
    
    public void Apply(UnbindTwitterAccountEvent unbindTwitterAccountEvent)
    {
        Token = "";
        TokenSecret = "";
    }
}