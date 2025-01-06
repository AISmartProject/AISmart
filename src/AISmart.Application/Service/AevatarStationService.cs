using System;
using System.Threading.Tasks;
using AISmart.CQRS.Provider;
using AISmart.GAgent.AtomicAgent.Agent;
using AISmart.GAgent.AtomicAgent.Dtos;
using AISmart.GAgent.AtomicAgent.Models;
using Microsoft.Extensions.Logging;
using Orleans;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.ObjectMapping;

namespace AISmart.Service;

public class AevatarStationService : ApplicationService, IAevatarStationService
{
    private readonly IClusterClient _clusterClient;
    private readonly ICQRSProvider _cqrsProvider;
    private readonly ILogger<AevatarStationService> _logger;
    private readonly IObjectMapper _objectMapper;

    public AevatarStationService(
        IClusterClient clusterClient, 
        ICQRSProvider cqrsProvider, 
        ILogger<AevatarStationService> logger,  
        IObjectMapper objectMapper)
    {
        _clusterClient = clusterClient;
        _cqrsProvider = cqrsProvider;
        _logger = logger;
        _objectMapper = objectMapper;
    }
    
    public async Task<AtomicAgentDto> GetAgentAsync(string id)
    {
        if (!Guid.TryParse(id, out Guid validGuid))
        {
            _logger.LogInformation("GetAgentAsync Invalid id: {id}", id);
            throw new UserFriendlyException("Invalid id");
        }
        
        var atomicAgent = _clusterClient.GetGrain<IAtomicGAgent>(validGuid);
        var agentData = await atomicAgent.GetAgentAsync();
        if (agentData == null)
        {
            _logger.LogInformation("GetAgentAsync agentProperty is null: {id}", id);
            throw new UserFriendlyException("agent not exist");
        }

        var resp = new AtomicAgentDto()
        {
            Id = agentData.Id.ToString(),
            Type = agentData.Type,
            Name = agentData.Name
        };
        
        switch (agentData.Type)
        {
            case AgentType.AiBasic:
                resp.Properties = _objectMapper.Map<AIBasicAgentData, AgentPropertyDto>(agentData.AIBasicAgentData);
                break;
            case AgentType.TelegramMessaging:
                resp.Properties = _objectMapper.Map<TelegramAgentData, AgentPropertyDto>(agentData.TelegramAgentData);
                break;
            case AgentType.TwitterMessaging:
                resp.Properties = _objectMapper.Map<TwitterAgentData, AgentPropertyDto>(agentData.TwitterAgentData);
                break;
            default:
                throw new UserFriendlyException("Invalid agent type");
        }
        
        return resp;
    }

    public async Task<AtomicAgentDto> CreateAgentAsync(CreateAtomicAgentDto createDto)
    {
        var address = GetCurrentUserAddress();
        var guid = Guid.NewGuid();
        var atomicAgent = _clusterClient.GetGrain<IAtomicGAgent>(guid);
        var agentData = _objectMapper.Map<CreateAtomicAgentDto, AgentData>(createDto);
        agentData.Id = guid;

        switch (createDto.Type)
        {
            case AgentType.AiBasic:
                var aiBasicAgentData = _objectMapper.Map<AgentPropertyDto, AIBasicAgentData>(createDto.Properties);
                agentData.AIBasicAgentData = aiBasicAgentData;
                await atomicAgent.CreateAgentAsync(agentData, address);
                _logger.LogInformation("CreateAgentAsync AIBasicAgentData: {aiBasicAgentData}", agentData);
                break;
            case AgentType.TelegramMessaging:
                var telegramAgentData = _objectMapper.Map<AgentPropertyDto, TelegramAgentData>(createDto.Properties);
                agentData.TelegramAgentData = telegramAgentData;
                await atomicAgent.CreateAgentAsync(agentData, address);
                break;
            case AgentType.TwitterMessaging:
                var twitterAgentData = _objectMapper.Map<AgentPropertyDto, TwitterAgentData>(createDto.Properties);
                agentData.TwitterAgentData = twitterAgentData;
                await atomicAgent.CreateAgentAsync(agentData, address);
                break;
            default:
                throw new UserFriendlyException("Invalid agent type");
        }
        
        _logger.LogInformation("CreateAgentAsync: {agentData}", agentData);
        
        return _objectMapper.Map<CreateAtomicAgentDto, AtomicAgentDto>(createDto);
    }

    public async Task<AtomicAgentDto> UpdateAgentAsync(string id, UpdateAtomicAgentDto updateDto)
    {
        if (!Guid.TryParse(id, out Guid validGuid))
        {
            _logger.LogInformation("UpdateAgentAsync Invalid id: {id}", id);
            throw new UserFriendlyException("Invalid id");
        }
        
        var atomicAgent = _clusterClient.GetGrain<IAtomicGAgent>(validGuid);
        var agentData = await atomicAgent.GetAgentAsync();
        
        if (agentData == null)
        {
            _logger.LogInformation("UpdateAgentAsync agentProperty is null: {id}", id);
            throw new UserFriendlyException("agent not exist");
        }
        
        var resp = new AtomicAgentDto()
        {
            Id = agentData.Id.ToString(),
            Type = agentData.Type
        };
        
        switch (agentData.Type)
        {
            case AgentType.AiBasic:
                _objectMapper.Map(updateDto.Properties, agentData.AIBasicAgentData);
                resp.Properties = _objectMapper.Map<AIBasicAgentData, AgentPropertyDto>(agentData.AIBasicAgentData);
                break;
            case AgentType.TelegramMessaging:
                _objectMapper.Map(updateDto.Properties, agentData.TelegramAgentData);
                resp.Properties = _objectMapper.Map<TelegramAgentData, AgentPropertyDto>(agentData.TelegramAgentData);
                break;
            case AgentType.TwitterMessaging:
                _objectMapper.Map(updateDto.Properties, agentData.TwitterAgentData);
                resp.Properties = _objectMapper.Map<TwitterAgentData, AgentPropertyDto>(agentData.TwitterAgentData);
                break;
            default:
                throw new UserFriendlyException("Invalid agent type");
        }
        
        // _objectMapper.Map(updateDto.Properties, agentData);
        if (!updateDto.Name.IsNullOrEmpty())
        {
            agentData.Name = updateDto.Name;
        }
        
        await atomicAgent.UpdateAgentAsync(agentData);
        resp.Name = agentData.Name;

        return resp;
    }

    public async Task DeleteAgentAsync(string id)
    {
        if (!Guid.TryParse(id, out Guid validGuid))
        {
            _logger.LogInformation("DeleteAgentAsync Invalid id: {id}", id);
            throw new UserFriendlyException("Invalid id");
        }
        
        var atomicAgent = _clusterClient.GetGrain<IAtomicGAgent>(validGuid);
        await atomicAgent.DeleteAgentAsync();
    }

    private string GetCurrentUserAddress()
    {
         // todo
        return "my_address";
    }
    
    
}