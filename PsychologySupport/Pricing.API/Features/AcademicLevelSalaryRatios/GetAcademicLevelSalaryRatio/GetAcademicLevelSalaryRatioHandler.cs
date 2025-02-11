using BuildingBlocks.CQRS;
using FluentValidation;
using Pricing.API.Data;
using Pricing.API.Exceptions;
using Pricing.API.Modules;

namespace Pricing.API.Features.AcademicLevelSalaryRatios.GetAcademicLevelSalaryRatio
{
        public record GetAcademicLevelSalaryRatioQuery(Guid Id) : IQuery<GetAcademicLevelSalaryRatioResult>;

        public record GetAcademicLevelSalaryRatioResult(AcademicLevelSalaryRatio AcademicLevelSalaryRatio);

        public class GetAcademicLevelSalaryRatioQueryValidator : AbstractValidator<GetAcademicLevelSalaryRatioQuery>
        {
            public GetAcademicLevelSalaryRatioQueryValidator()
            {
                RuleFor(q => q.Id).NotEmpty().WithMessage("Id không được để trống.");
            }
        }

        public class GetAcademicLevelSalaryRatioHandler : IQueryHandler<GetAcademicLevelSalaryRatioQuery, GetAcademicLevelSalaryRatioResult>
        {
            private readonly PricingDbContext _context;

            public GetAcademicLevelSalaryRatioHandler(PricingDbContext context)
            {
                _context = context;
            }

            public async Task<GetAcademicLevelSalaryRatioResult> Handle(GetAcademicLevelSalaryRatioQuery query, CancellationToken cancellationToken)
            {
                var ratio = await _context.AcademicLevelSalaryRatios
                                .FindAsync(query.Id)
                            ?? throw new PricingNotFoundException(query.Id.ToString());

                return new GetAcademicLevelSalaryRatioResult(ratio);
            }
        }
 }
