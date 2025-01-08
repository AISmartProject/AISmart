using AISmart.Agents;
using MediatR;

namespace AISmart.CQRS.Dto;

public class SendEventCommand : IRequest
{
    public EventBase Event { get; set; }
}
