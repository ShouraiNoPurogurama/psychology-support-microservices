using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Pricing.API.Data;
using Pricing.API.Models;

namespace Pricing.API.Features.AcademicLevelSalaryRatios.GetAcademicLevelSalaryRatios;

public record GetAcademicLevelSalaryRatiosQuery(int PageNumber, int PageSize) : IQuery<GetAcademicLevelSalaryRatiosResult>;

public record GetAcademicLevelSalaryRatiosResult(IEnumerable<AcademicLevelSalaryRatio> SalaryRatios, int TotalCount);

public class GetAcademicLevelSalaryRatiosHandler : IQueryHandler<GetAcademicLevelSalaryRatiosQuery, GetAcademicLevelSalaryRatiosResult>
{
    private readonly PricingDbContext _dbContext;

    public GetAcademicLevelSalaryRatiosHandler(PricingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetAcademicLevelSalaryRatiosResult> Handle(GetAcademicLevelSalaryRatiosQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;
        var totalCount = await _dbContext.AcademicLevelSalaryRatios.CountAsync(cancellationToken);

        var salaryRatios = await _dbContext.AcademicLevelSalaryRatios
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new GetAcademicLevelSalaryRatiosResult(salaryRatios, totalCount);
    }
}
