using AISmart.Domain.Grains.Event;
using AutoMapper;

namespace AISmart.Application.Grains;

public class AISmartGrainsAutoMapperProfile : Profile
{
    public AISmartGrainsAutoMapperProfile()
    {
        // User AutoMap
        CreateMap<BasicEvent, TelegramEvent>().ReverseMap();
        CreateMap<AgentTaskState, AgentTaskDto>().ReverseMap();
    }
}