using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Test.Application.Data;
using Test.Application.Dtos;

namespace Test.Application.Tests.Queries
{
    public record GetTestResultQuery(Guid TestResultId)
    : IQuery<GetTestResultResult>;
    public record GetTestResultResult(TestResultDto TestResult);

    public class GetTestResultsHandler
    : IQueryHandler<GetTestResultQuery, GetTestResultResult>
    {
        private readonly ITestDbContext _context;

        public GetTestResultsHandler(ITestDbContext context)
        {
            _context = context;
        }

        public async Task<GetTestResultResult> Handle(
            GetTestResultQuery request, CancellationToken cancellationToken)
        {
            var testResult = await _context.TestResults
                .Where(tr => tr.Id == request.TestResultId)
                .FirstOrDefaultAsync(cancellationToken);

            if (testResult == null)
            {
                throw new NotFoundException($"Không tìm thấy kết quả bài Test với ID là {request.TestResultId}.");
            }

            var testResultDto = testResult.Adapt<TestResultDto>();

            return new GetTestResultResult(testResultDto);
        }
    }
}
