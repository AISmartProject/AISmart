using MediatR;

namespace AISmart.CQRS.Dto;

public class GetGEventQuery : IRequest<string>
{
    public string Id { get; set; }
    public string Index { get; set; }
}