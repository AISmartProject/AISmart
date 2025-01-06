using AISmart.CQRS.Dto;
using AutoMapper;

namespace AISmart;

public class AISmartCQRSAutoMapperProfile : Profile
{
    public AISmartCQRSAutoMapperProfile()
    {
        CreateMap<GetLogQuery, ChatLogQueryInputDto>();
        CreateMap<SaveLogCommand, AIChatLogIndex>();
        CreateMap<AIChatLogIndex, ChatLogPageResultDto>();
    }
}
