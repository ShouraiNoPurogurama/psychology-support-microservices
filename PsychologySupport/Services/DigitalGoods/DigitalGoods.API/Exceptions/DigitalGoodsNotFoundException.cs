using BuildingBlocks.Exceptions;

namespace DigitalGoods.API.Exceptions
{
    public class DigitalGoodsNotFoundException : NotFoundException
    {
        public DigitalGoodsNotFoundException(string? message) : base(message)
        {
        }

        public DigitalGoodsNotFoundException(string name, Guid id) : base($"Entity \"{name}\" with Id {id} was not found.")
        {
        }
    }
}
