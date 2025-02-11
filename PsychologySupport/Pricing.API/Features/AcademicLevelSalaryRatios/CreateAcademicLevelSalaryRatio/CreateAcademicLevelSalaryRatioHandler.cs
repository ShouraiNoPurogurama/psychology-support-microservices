using BuildingBlocks.CQRS;
using Pricing.API.Data;
using Pricing.API.Modules;

namespace Pricing.API.Features.AcademicLevelSalaryRatios.CreateAcademicLevelSalaryRatio;

public record CreateAcademicLevelSalaryRatioCommand(AcademicLevelSalaryRatio SalaryRatio) : ICommand<CreateAcademicLevelSalaryRatioResult>;

public record CreateAcademicLevelSalaryRatioResult(Guid Id);

public class CreateAcademicLevelSalaryRatioHandler : ICommandHandler<CreateAcademicLevelSalaryRatioCommand, CreateAcademicLevelSalaryRatioResult>
{
    private readonly PricingDbContext _context;

    public CreateAcademicLevelSalaryRatioHandler(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<CreateAcademicLevelSalaryRatioResult> Handle(CreateAcademicLevelSalaryRatioCommand request, CancellationToken cancellationToken)
    {
        _context.AcademicLevelSalaryRatios.Add(request.SalaryRatio);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateAcademicLevelSalaryRatioResult(request.SalaryRatio.Id);
    }
}
