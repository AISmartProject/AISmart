
using System;

namespace AISmart.Dto;

public abstract class BaseEventDto
{
    public Guid Id { get; set; }

    public DateTime Ctime { get; set; }

}