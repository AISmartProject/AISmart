using AISmart.Agents;
using MediatR;

namespace AISmart.CQRS.Dto;

public class SaveStateCommand : IRequest
{
    public string Id { get; set; }
    public StateBase State { get; set; }
}
