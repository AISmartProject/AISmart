using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.CQRS.Provider;
using AISmart.GAgent.AtomicAgent.Agent;
using AISmart.GAgent.AtomicAgent.Dtos;
using AISmart.GAgent.AtomicAgent.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
            Name = agentData.Name,
            Properties = JsonConvert.DeserializeObject<Dictionary<string, string>>(agentData.Properties)
        };
        
        return resp;
    }

    public async Task<AtomicAgentDto> CreateAgentAsync(CreateAtomicAgentDto createDto)
    {
        var address = GetCurrentUserAddress();
        var guid = Guid.NewGuid();
        var atomicAgent = _clusterClient.GetGrain<IAtomicGAgent>(guid);
        var agentData = _objectMapper.Map<CreateAtomicAgentDto, AgentData>(createDto);
        agentData.Id = guid;
        agentData.Properties = JsonConvert.SerializeObject(createDto.Properties);
        await atomicAgent.CreateAgentAsync(agentData, address);
        var resp = _objectMapper.Map<CreateAtomicAgentDto, AtomicAgentDto>(createDto);
        resp.Id = guid.ToString();
        return resp;
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
        
        if (!updateDto.Name.IsNullOrEmpty())
        {
            agentData.Name = updateDto.Name;
        }

        var newProperties = JsonConvert.DeserializeObject<Dictionary<string, string>>(agentData.Properties);
        if (newProperties != null)
        {
            foreach (var kvp in updateDto.Properties)
            {
                if (newProperties.ContainsKey(kvp.Key))
                {
                    newProperties[kvp.Key] = kvp.Value;
                }
            }
            
            agentData.Properties = JsonConvert.SerializeObject(newProperties);
        }
        
        await atomicAgent.UpdateAgentAsync(agentData);
        resp.Name = agentData.Name;
        resp.Properties = newProperties;

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