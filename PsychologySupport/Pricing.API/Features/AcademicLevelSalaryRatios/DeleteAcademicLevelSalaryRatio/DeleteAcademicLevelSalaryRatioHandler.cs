using BuildingBlocks.CQRS;
using Pricing.API.Data;
using Pricing.API.Exceptions;

namespace Pricing.API.Features.AcademicLevelSalaryRatios.DeleteAcademicLevelSalaryRatio;

public record DeleteAcademicLevelSalaryRatioCommand(Guid Id) : ICommand<DeleteAcademicLevelSalaryRatioResult>;

public record DeleteAcademicLevelSalaryRatioResult(bool IsSuccess);

public class DeleteAcademicLevelSalaryRatioHandler : ICommandHandler<DeleteAcademicLevelSalaryRatioCommand, DeleteAcademicLevelSalaryRatioResult>
{
    private readonly PricingDbContext _context;

    public DeleteAcademicLevelSalaryRatioHandler(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteAcademicLevelSalaryRatioResult> Handle(DeleteAcademicLevelSalaryRatioCommand request, CancellationToken cancellationToken)
    {
        var existingRecord = await _context.AcademicLevelSalaryRatios.FindAsync(request.Id)
                              ?? throw new PricingNotFoundException("Academic Level Salary Ratio", request.Id);

        _context.AcademicLevelSalaryRatios.Remove(existingRecord);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        return new DeleteAcademicLevelSalaryRatioResult(result);
    }
}
