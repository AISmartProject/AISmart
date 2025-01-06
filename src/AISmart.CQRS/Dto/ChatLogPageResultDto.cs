using System.Collections.Generic;

namespace AISmart.CQRS.Dto;

public class ChatLogPageResultDto
{
    public long TotalRecordCount { get; set; }
    public List<AIChatLogIndexDto> Data{ get; set; }
}