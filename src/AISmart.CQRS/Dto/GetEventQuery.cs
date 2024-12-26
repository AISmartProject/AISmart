using MediatR;

namespace AISmart.CQRS.Dto;

public class GetEventQuery : IRequest<BaseStateIndex>
{
    public string Id { get; set; }
    public string Index { get; set; }
}