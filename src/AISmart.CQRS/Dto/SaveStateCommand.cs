using AISmart.Agents;
using MediatR;

namespace AISmart.CQRS.Dto;

public class SaveStateCommand : IRequest<int>
{
    public string Id { get; set; }
    public StateBase State { get; set; }
}
