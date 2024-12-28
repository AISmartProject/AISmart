using AISmart.Agent.GEvents;
using AutoMapper;
using AISmart.Dto;
using AISmart.GAgent.Dto;

namespace AISmart;

public class AISmartApplicationAutoMapperProfile : Profile
{
    public AISmartApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        
        //Example related, can be removed
        CreateMap<CreateTransactionGEventDto, CreateTransactionGEvent>();
        CreateMap<CreateTransactionGEvent, CreateTransactionGEventDto>();
        CreateMap<BindTwitterAccountGEvent, BindTwitterAccountGEventDto>();
        CreateMap<BindTwitterAccountGEventDto, BindTwitterAccountGEvent>();
        CreateMap<PumpFunSendMessageGEvent, PumpFunSendMessageGEventDto>();
        CreateMap<PumpFunSendMessageGEventDto, PumpFunSendMessageGEvent>();
    }
}
