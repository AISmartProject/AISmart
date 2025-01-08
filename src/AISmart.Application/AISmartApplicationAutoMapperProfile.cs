using AutoMapper;
using AISmart.Authors;

namespace AISmart;

public class AISmartApplicationAutoMapperProfile : Profile
{
    public AISmartApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        
        //Example related, can be removed
        CreateMap<Author, AuthorDto>();

    }
}
