using BuildingBlocks.DDD;

namespace Test.Domain.Models;

public class Category : AuditableEntity<Guid>
{
    private Category()
    {
    }

    public Category(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
}