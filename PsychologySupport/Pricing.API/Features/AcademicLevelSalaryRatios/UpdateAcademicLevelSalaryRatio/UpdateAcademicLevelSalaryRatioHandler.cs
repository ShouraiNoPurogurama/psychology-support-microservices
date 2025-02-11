using BuildingBlocks.CQRS;
using Mapster;
using Pricing.API.Data;
using Pricing.API.Dtos;
using Pricing.API.Exceptions;
using Pricing.API.Modules;

namespace Pricing.API.Features.AcademicLevelSalaryRatios.UpdateAcademicLevelSalaryRatio;

public record UpdateAcademicLevelSalaryRatioCommand(AcademicLevelSalaryRatioDto AcademicLevelSalaryRatio) : ICommand<UpdateAcademicLevelSalaryRatioResult>;

public record UpdateAcademicLevelSalaryRatioResult(bool IsSuccess);

public class UpdateAcademicLevelSalaryRatioHandler : ICommandHandler<UpdateAcademicLevelSalaryRatioCommand, UpdateAcademicLevelSalaryRatioResult>
{
    private readonly PricingDbContext _context;

    public UpdateAcademicLevelSalaryRatioHandler(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateAcademicLevelSalaryRatioResult> Handle(UpdateAcademicLevelSalaryRatioCommand request, CancellationToken cancellationToken)
    {
        var existingRatio = await _context.AcademicLevelSalaryRatios.FindAsync(request.AcademicLevelSalaryRatio.Id)
                             ?? throw new PricingNotFoundException("Academic Level Salary Ratio", request.AcademicLevelSalaryRatio.Id);

        existingRatio = request.AcademicLevelSalaryRatio.Adapt(existingRatio);

        _context.Update(existingRatio);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        return new UpdateAcademicLevelSalaryRatioResult(result);
    }
}
