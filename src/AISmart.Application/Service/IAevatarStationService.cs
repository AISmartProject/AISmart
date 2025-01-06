using System;
using System.Threading.Tasks;
using AISmart.GAgent.AtomicAgent.Dtos;

namespace AISmart.Service;

public interface IAevatarStationService
{
    Task<AtomicAgentDto> GetAgentAsync(string id);
    Task<AtomicAgentDto> CreateAgentAsync(CreateAtomicAgentDto createDto);
    Task<AtomicAgentDto> UpdateAgentAsync(string id, UpdateAtomicAgentDto updateDto);
    Task DeleteAgentAsync(string id);
}