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
        var dic = new Dictionary<Guid, List<MicroAIMessage>>();
        var contentArr = new string[]
        {
            "Name:BlockNet;Reason:The core characteristics of a blockchain system are 'blocks' and 'networks (chains).' BlockNet directly reflects these features, emphasizing the technical foundation of the system. It is simple, easy to remember, and highly fitting to the technical context.",
            "Name:LedgerSphere;Reason:A blockchain is essentially a decentralized ledger system, and 'Ledger' is an indispensable part of finance. Combined with 'Sphere' (meaning globe or globalization), it highlights the decentralization feature of blockchain systems and their global adaptability, showcasing its core potential in the financial domain.",
            "Name:CryptoWeave;Reason:The essence of blockchain lies in its ability to 'weave' transactions and data into an indivisible, transparent, and secure network through cryptography. The name 'CryptoWeave' creates a strong imagery, emphasizing the cryptographic security of blockchain, while hinting at the interconnected and organic nature of the network.",
            "Name:ChainNova;Reason:'Chain' represents blockchain, and 'Nova' means 'new star,' symbolizing blockchain as a rapidly emerging part of new-age technology. It evokes a sense of futurism, and the name itself sounds highly brandable and has strong promotional potential.",
            "Name:DecentraLink;Reason:'Decentra' represents the core feature of blockchain: decentralization. 'Link' signifies its ability to connect people, systems, assets, and information. This name accurately captures the technical architecture of blockchain systems while exuding a modern technological vibe.",
            "Name:BlockHaven;Reason:'Block' reflects the core of blockchain technology, while 'Haven' means 'safe harbor,' signifying that blockchain creates a secure, transparent, and dependable environment for users. This name strongly emphasizes blockchain’s advantages and gives the impression of a trustworthy platform, making it suitable for a project targeting everyday users."
        };
        for (var i = 0; i < 5; i++)
        {
            var guid = Guid.NewGuid();
            var messages = new List<MicroAIMessage>();
            messages.Add(new MicroAIMessage("player", contentArr[i]));
            dic.Add(guid, messages);
        }
        var input = new SingleVoteCharmingEvent()
        {
           // VoteMessage = dic,
            Round = 1
        };
        await grain.VoteAgentAsync(input);
        return null;
    }
}