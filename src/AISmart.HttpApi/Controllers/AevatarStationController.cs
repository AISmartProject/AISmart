using System;
using System.Threading.Tasks;
using AISmart.GAgent.AtomicAgent.Dtos;
using AISmart.Service;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AISmart.Controllers;

[Area("app")]
[ControllerName("aevatar")]
[Route("api/aevatar")]
public class AevatarStationController
{
    private readonly ILogger<AevatarStationController> _logger;
    private readonly IAevatarStationService  _aevatarStationService;
    
    public AevatarStationController(
        ILogger<AevatarStationController> logger, 
        IAevatarStationService aevatarStationService)
    {
        _logger = logger;
        _aevatarStationService = aevatarStationService;
    }
    
    [HttpPost]
    public async Task<AtomicAgentDto> CreateAgent([FromBody] CreateAtomicAgentDto createAtomicAgentDto)
    {
        _logger.LogInformation("Create Atomic-Agent: {agent}", JsonConvert.SerializeObject(createAtomicAgentDto));
        var agentDto = await _aevatarStationService.CreateAgentAsync(createAtomicAgentDto);
        return agentDto;
    }
    
    [HttpGet("{id}")]
    public async Task<AtomicAgentDto> GetAgent(string id)
    {
        _logger.LogInformation("Get Atomic-Agent: {agent}", id);
        var agentDto = await _aevatarStationService.GetAgentAsync(id);
        return agentDto;
    }
    
    [HttpPut("{id}")]
    public async Task<AtomicAgentDto> UpdateAgent(string id, [FromBody] UpdateAtomicAgentDto updateAtomicAgentDto)
    {
        _logger.LogInformation("Update Atomic-Agent: {agent}", JsonConvert.SerializeObject(updateAtomicAgentDto));
        var agentDto = await _aevatarStationService.UpdateAgentAsync(id, updateAtomicAgentDto);
        return agentDto;
    }

   
    [HttpDelete("{id}")]
    public async Task DeleteAgent(string id)
    {
        _logger.LogInformation("Delete Atomic-Agent: {agent}", id);
        await _aevatarStationService.DeleteAgentAsync(id);
    }
    
}