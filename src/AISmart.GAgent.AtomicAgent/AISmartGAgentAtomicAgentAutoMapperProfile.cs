using AISmart.GAgent.AtomicAgent.Dtos;
using AISmart.GAgent.AtomicAgent.Models;
using AutoMapper;

namespace AISmart.GAgent.AtomicAgent;

public class AISmartGAgentAtomicAgentAutoMapperProfile : Profile
{
    public AISmartGAgentAtomicAgentAutoMapperProfile()
    {
        CreateMap<CreateAtomicAgentDto, AtomicAgentDto>().ReverseMap();
        CreateMap<AgentData, CreateAtomicAgentDto>().ReverseMap();
    }
}