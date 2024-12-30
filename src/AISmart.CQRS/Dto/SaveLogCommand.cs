using MediatR;
using DateTime = Google.Type.DateTime;

namespace AISmart.CQRS.Dto;

public class SaveLogCommand : IRequest
{
    public string Id{ get; set; }
    public string GroupId { get; set; }
    public string From { get; set; }
    public string Request { get; set; }
    public string Response { get; set; }
    public DateTime Ctime { get; set; }
}
