using Volo.Abp.Application.Dtos;

namespace AISmart.Authors;

public class GetAuthorListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
