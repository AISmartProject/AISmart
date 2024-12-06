using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AISmart.AgentTask;
using AISmart.Application.Grains.Event;
using AISmart.Dapr;
using AISmart.Domain.Grains.Event;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Orleans;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AISmart.Samples;

public class AgentTaskServiceTests : AISmartApplicationTestBase

{
    private readonly AgentTaskService _agentTaskService;
    private readonly IClusterClient _clusterClient;
    private readonly ITestOutputHelper _output;

    private readonly Guid _tgTemplateId;
    private readonly ITelegramAgent _tgAgent;
    
    private readonly Guid _marketTemplateId;
    private readonly IMarketLeaderAgent _marketLeaderAgent;
    
    private readonly Guid _marketOperatorTemplateId;
    private readonly IMarketOperatorAgent _marketOperatorAgent;
    
    private readonly Guid _senderTemplateId;
    private readonly IAgent _senderAgent;
    
    private readonly Guid _receiverTemplateId;
    private readonly IAgent _receiverAgent;

    public AgentTaskServiceTests(ITestOutputHelper output)
    {
        _agentTaskService = GetRequiredService<AgentTaskService>();
        _clusterClient = GetRequiredService<IClusterClient>();
        _output = output;
        
        
        
        _tgTemplateId = Guid.NewGuid();
        _tgAgent = _clusterClient.GetGrain<ITelegramAgent>(_tgTemplateId);
        
        _marketTemplateId = Guid.NewGuid();
        _marketLeaderAgent = _clusterClient.GetGrain<IMarketLeaderAgent>(_marketTemplateId);
        
        _marketOperatorTemplateId = Guid.NewGuid();
        _marketOperatorAgent = _clusterClient.GetGrain<IMarketOperatorAgent>(_marketOperatorTemplateId);
        
        
        
        _senderTemplateId = Guid.NewGuid();
        _senderAgent = _clusterClient.GetGrain<IAgent>(_senderTemplateId);
        
        _receiverTemplateId = Guid.NewGuid();
        _receiverAgent = _clusterClient.GetGrain<IAgent>(_receiverTemplateId);
        
        
    }
    
    
    [Fact]
    public async Task EventBus_Test()
    {
        BasicEvent basicEvent = new BasicEvent
        {
            AgentTopic = CommonConstants.CommonTopic,
            Downstreams = null,
            Content = "common message"
        };
        
        await _senderAgent.PublishAsync(basicEvent);
        
        await _receiverAgent.HandleEventAsync(basicEvent);
        
    }
    

    [Fact]
    public async Task Telegram_Test()
    {
        TelegramEvent telegramEvent = new TelegramEvent
        {
            Id = _tgTemplateId,
            AgentTopic = CommonConstants.TelegramTopic,
            Downstreams = null,
            Content = "比特币突破10万美元大关"
        };
        await _tgAgent.ChatAsync(telegramEvent);
        
        await _marketLeaderAgent.ExecuteStrategyAsync(telegramEvent);


    }
    
    [Fact]
    public async Task Multi_Agent_Test()
    {
        TelegramEvent telegramEvent = new TelegramEvent
        {
            Id = _tgTemplateId,
            AgentTopic = CommonConstants.TelegramTopic,
            Downstreams = null,
            Content = "比特币突破10万美元大关"
        };
        await _tgAgent.ChatAsync(telegramEvent);
        
        await _marketLeaderAgent.ExecuteStrategyAsync(telegramEvent);
        

        await _marketOperatorAgent.AnalyseContentAsync(new MarketLeaderCreatedEvent());
        await _marketOperatorAgent.CompleteAnalyseContentAsync();

        await _marketLeaderAgent.CompelteStrategyAsync();

    }
}