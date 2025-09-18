using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Data;
using Payment.Application.Payments.Dtos;
using Payment.Domain.Enums;

namespace Payment.Application.Payments.Queries
{
    public record GetAllPaymentsQuery(
         int PageIndex,
         int PageSize,
         Guid? PatientProfileId,
         PaymentStatus? Status,
         DateOnly? CreatedAt,
         PaymentType? PaymentType,
         string? SortOrder
    ) : IQuery<GetAllPaymentsResult>;

    public record GetAllPaymentsResult(PaginatedResult<PaymentDto> Payments);

    public class GetAllPaymentsHandler(IPaymentDbContext dbContext) : IQueryHandler<GetAllPaymentsQuery, GetAllPaymentsResult>
    {
        public async Task<GetAllPaymentsResult> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
        {
            var pageSize = request.PageSize;
            var pageIndex = Math.Max(0, request.PageIndex - 1);

            var query = dbContext.Payments
                .Include(p => p.PaymentDetails)
                .AsQueryable();

            // Filtering
            if (request.PatientProfileId.HasValue)
                query = query.Where(p => p.PatientProfileId == request.PatientProfileId);

            if (request.Status.HasValue)
                query = query.Where(p => p.Status == request.Status);

            if (request.CreatedAt.HasValue)
            {
                var requestDate = request.CreatedAt.Value;
                query = query.Where(p => DateOnly.FromDateTime(p.CreatedAt.UtcDateTime) == requestDate);
            }


            if (request.PaymentType.HasValue)
                query = query.Where(p => p.PaymentType == request.PaymentType);

            // Sorting
            query = request.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt);

            // Pagination
            var totalCount = await query.CountAsync(cancellationToken);
            var payments = await query.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            var paginatedResult = new PaginatedResult<PaymentDto>(
                pageIndex + 1,
                pageSize,
                totalCount,
                payments.Adapt<IEnumerable<PaymentDto>>()
            );

            return new GetAllPaymentsResult(paginatedResult);
        }
    }

}
