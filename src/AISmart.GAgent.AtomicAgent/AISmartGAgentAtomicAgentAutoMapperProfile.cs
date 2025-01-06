using AISmart.GAgent.AtomicAgent.Dtos;
using AISmart.GAgent.AtomicAgent.Models;
using AutoMapper;

namespace AISmart.GAgent.AtomicAgent;

public class AISmartGAgentAtomicAgentAutoMapperProfile : Profile
{
    public AISmartGAgentAtomicAgentAutoMapperProfile()
    {
        CreateMap<AgentData, AgentPropertyDto>().ReverseMap();
        CreateMap<AIBasicAgentData, AgentPropertyDto>().ReverseMap().ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
        {
            if (srcMember is List<string> list)
            {
                return list.Any(); 
            }
            
            if (srcMember is string str)
            {
                return !string.IsNullOrEmpty(str);
            }
            
            return srcMember != null;
        }));
        CreateMap<TelegramAgentData, AgentPropertyDto>().ReverseMap().ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
        {
            if (srcMember is List<string> list)
            {
                return list.Any(); 
            }
            
            if (srcMember is string str)
            {
                return !string.IsNullOrEmpty(str);
            }
            
            return srcMember != null;
        }));
        CreateMap<TwitterAgentData, AgentPropertyDto>().ReverseMap().ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
        {
            if (srcMember is List<string> list)
            {
                return list.Any(); 
            }
            
            if (srcMember is string str)
            {
                return !string.IsNullOrEmpty(str);
            }
            
            return srcMember != null;
        }));
        CreateMap<CreateAtomicAgentDto, AtomicAgentDto>().ReverseMap();
        CreateMap<AgentData, CreateAtomicAgentDto>().ReverseMap();
    }
}