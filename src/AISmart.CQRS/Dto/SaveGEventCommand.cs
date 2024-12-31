using AISmart.Agents;
using MediatR;

namespace AISmart.CQRS.Dto;


public class SaveGEventCommand : IRequest
{
    public string Id { get; set; }
    public GEventBase GEvent { get; set; }
}
