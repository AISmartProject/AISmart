
using System;

namespace AISmart.CQRS.Dto;

public abstract class BaseEventDto
{
    public Guid Id { get; set; }

    public DateTime Ctime { get; set; }

}