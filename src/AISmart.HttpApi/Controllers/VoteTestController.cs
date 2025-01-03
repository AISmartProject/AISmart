using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Agent.GEvents;
using AISmart.Application;
using AISmart.Dto;
using AiSmart.GAgent.TestAgent.NamingContest.VoteAgent;
using AiSmart.GAgent.TestAgent.NamingContest.VoteAgent.Grains;
using AISmart.Service;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using Volo.Abp;

namespace AISmart.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("VoteTest")]
public class VoteTestController : AISmartController
{
    private readonly ILogger<VoteTestController> _logger;
    private readonly IDemoAppService _demoAppService;
    private readonly ICqrsService _cqrsService;
    private readonly IClusterClient _clusterClient;

    public VoteTestController(ILogger<VoteTestController> logger,
        IDemoAppService demoAppService,ICqrsService cqrsService,IClusterClient clusterClient)
    {
        _logger = logger;
        _demoAppService = demoAppService;
        _cqrsService = cqrsService;
        _clusterClient = clusterClient;
    }
    

    [HttpGet("vote/send")]
    public async Task<CreateTransactionGEventDto> PostMessages(string message)
    {
        var grain = _clusterClient.GetGrain<IVoteAgentGrain>(Guid.NewGuid());
        var contentArr = new string[]
        {
           "james named the above information is: \"LingoLink\".",
           "kob named the above information is: \"ChatBridge\".",
           "james debate is: \"\"LingoLink\" and \"ChatBridge\" are decent suggestions, but they fall short in capturing the vibrant, youthful energy needed to appeal to young users. \"LingoLink\" sounds functional but lacks excitement, and \"ChatBridge\" feels too generic, focusing narrowly on communication rather than the fun, innovative aspect.\n\nMy suggested name would aim to be more dynamic, catchy, and appealing to a younger audience, something that embodies both functionality and modern style. How about \"VibeVerse\"? It combines the vibe of youth with the universality of languages, making it memorable and marketable.\".",
           "kob debate is: \"\"LingoLink\" and \"ChatBridge\" are functional but lack the spark to captivate young audiences. \"LingoLink\" feels too utilitarian, failing to excite or stand out, while \"ChatBridge\" is overly generic, offering no distinction or youthful appeal. My choice, \"VibeVerse,\" encapsulates the energy, inclusivity, and modernity that resonates with younger users, making it both engaging and unforgettable. It bridges functionality with a vibrant identity, perfect for the target market.\".",
           "kob debate is: \"Both \"LingoLink\" and \"ChatBridge\" are practical but uninspired. \"LingoLink\" is overly technical and lacks emotional resonance, while \"ChatBridge\" is bland and doesn't convey innovation or excitement. \n\n\"VibeVerse,\" on the other hand, is dynamic and memorable. It combines the youthful energy (\"vibe\") with the universal appeal of language (\"verse\"), perfectly aligning with the target audience's preferences. It’s not just functional but also aspirational, making it a stronger, marketable choice.\".",
           "james debate is: \"James: \n\n\"LingoLink\" is overly mechanical and misses the emotional and modern appeal young users crave. \"ChatBridge\" is generic, offering no innovative spark or memorable identity. Both lack the dynamic energy needed for the youth market. My suggestion, \"VibeVerse,\" is vibrant, memorable, and blends youthful energy with universal connection, making it aspirational and perfectly suited for the target demographic.\n\n---\n\nKob: \n\n\"LingoLink\" feels cold and overly technical, failing to engage emotionally. \"ChatBridge\" is uninspired and lacks distinction. Both names miss the mark in appealing to a youthful, vibrant audience. \"VibeVerse,\" however, combines energy, inclusivity, and innovation, creating a brand identity that resonates deeply with young users while being aspirational and marketable.\".",
           "james debate is: \"**James:**  \n\"LingoLink\" and \"ChatBridge\" are uninspired. \"LingoLink\" is too sterile and lacks emotional or youthful appeal. \"ChatBridge\" is generic and unimaginative, failing to stand out in a competitive market. My name, \"VibeVerse,\" captures youthful energy and universal connection, creating a vibrant and aspirational identity that resonates with young users.\n\n**Kob:**  \n\"LingoLink\" is overly functional and boring, while \"ChatBridge\" is bland and lacks innovation. Neither name aligns with the modern, dynamic spirit young audiences seek. \"VibeVerse\" is memorable, inclusive, and emotionally engaging, making it the superior choice for a brand targeting young, global users.\".",
           "kob debate is: \"**James:**  \n\"LingoLink\" is too utilitarian, lacking the vibrancy that appeals to young users. \"ChatBridge\" is generic and fails to evoke innovation or excitement. \"VibeVerse\" is superior because it’s dynamic, emotionally engaging, and resonates with the youthful, global audience. It’s memorable and aspirational, perfectly aligning with the product's purpose.\n\n**Kob:**  \n\"LingoLink\" is cold and mechanical, offering no emotional connection. \"ChatBridge\" is uninspired, overly simplistic, and fails to stand out. \"VibeVerse\" stands apart as it captures the energy, inclusivity, and modernity that young audiences desire, making it a vibrant and marketable choice.\"."
        };
       
        var messages = new List<MicroAIMessage>();
        for (var i = 0; i < 8; i++)
        {
            messages.Add(new MicroAIMessage("user", contentArr[i]));
        }
        var dic = new Dictionary<Guid, string>();
        dic.Add(Guid.Parse("b7b6468a-8d2a-4378-8ff8-e3dd59570148"), "james");
        dic.Add(Guid.Parse("ec3f1fcf-36c1-4e53-a51c-727c92b7f63b"), "kob");
        var input = new SingleVoteCharmingEvent()
        {
            VoteMessage = messages,
            Round = 1,
            AgentIdNameDictionary = dic
        };
        await grain.VoteAgentAsync(input);
        return null;
    }
}