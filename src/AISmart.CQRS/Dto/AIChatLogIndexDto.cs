using Nest;
using DateTime = Google.Type.DateTime;
using IRequest = MediatR.IRequest;

namespace AISmart.CQRS.Dto;

public class AIChatLogIndexDto : IRequest
{
    [Keyword] public string Id{ get; set; }
    [Keyword] public string GroupId { get; set; }
    [Keyword] public string From { get; set; }
    [Keyword] public string Request { get; set; }
    [Keyword] public string Response { get; set; }
    [Keyword] public DateTime Ctime { get; set; }
}
