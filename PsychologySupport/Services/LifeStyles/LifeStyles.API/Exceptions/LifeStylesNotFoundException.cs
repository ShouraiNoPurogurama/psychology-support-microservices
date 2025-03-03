using BuildingBlocks.Exceptions;

namespace LifeStyles.API.Exceptions;

public class LifeStylesNotFoundException : NotFoundException
{
    public LifeStylesNotFoundException(string? message) : base(message)
    {
    }

    public LifeStylesNotFoundException(string name, Guid id) : base($"Entity \"{name}\" with Id {id} was not found.")
    {
    }
}