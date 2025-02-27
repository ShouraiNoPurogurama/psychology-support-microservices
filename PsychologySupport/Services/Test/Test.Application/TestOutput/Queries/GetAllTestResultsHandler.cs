using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Application.Data;
using Test.Domain.Models;

namespace Test.Application.TestOutput.Queries
{
    public record GetAllTestResultsQuery(Guid PatientId, PaginationRequest PaginationRequest)
        : IQuery<GetAllTestResultsResult>;

    public record GetAllTestResultsResult(PaginatedResult<TestResult> TestResults);

    public class GetAllTestResultsHandler
        : IQueryHandler<GetAllTestResultsQuery, GetAllTestResultsResult>
    {
        private readonly ITestDbContext _context;

        public GetAllTestResultsHandler(ITestDbContext context) => _context = context;

        public async Task<GetAllTestResultsResult> Handle(
            GetAllTestResultsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.TestResults
                .AsNoTracking()
                .Where(tr => tr.PatientId == request.PatientId);

            var totalCount = await query.CountAsync(cancellationToken);

            var testResults = await query
                .Skip(request.PaginationRequest.PageIndex * request.PaginationRequest.PageSize)
                .Take(request.PaginationRequest.PageSize)
                .ToListAsync(cancellationToken);

            var paginatedResult = new PaginatedResult<TestResult>(
                request.PaginationRequest.PageIndex,
                request.PaginationRequest.PageSize,
                totalCount,
                testResults
            );

            return new GetAllTestResultsResult(paginatedResult);
        }
    }
}
