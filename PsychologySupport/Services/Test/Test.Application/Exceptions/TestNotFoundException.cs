using BuildingBlocks.Exceptions;

namespace Test.Application.Exceptions;

public class TestNotFoundException : NotFoundException
{
    public TestNotFoundException(Guid id) : base("Test", id)
    {
    }
}