using BuildingBlocks.CQRS;
using Pricing.API.Data;
using Pricing.API.Exceptions;

namespace Pricing.API.Features.ExperiencePriceRanges.DeleteExperiencePriceRange;

public record DeleteExperiencePriceRangeCommand(Guid Id) : ICommand<DeleteExperiencePriceRangeResult>;

public record DeleteExperiencePriceRangeResult(bool IsSuccess);

public class DeleteExperiencePriceRangeHandler : ICommandHandler<DeleteExperiencePriceRangeCommand, DeleteExperiencePriceRangeResult>
{
    private readonly PricingDbContext _context;

    public DeleteExperiencePriceRangeHandler(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteExperiencePriceRangeResult> Handle(DeleteExperiencePriceRangeCommand request, CancellationToken cancellationToken)
    {
        var experiencePriceRange = await _context.ExperiencePriceRanges.FindAsync(request.Id)
                                   ?? throw new PricingNotFoundException("Experience Price Range", request.Id);

        _context.ExperiencePriceRanges.Remove(experiencePriceRange);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        return new DeleteExperiencePriceRangeResult(result);
    }
}
