using MediatR;
using Microsoft.EntityFrameworkCore;
using Test.Application.Data;
using Test.Domain.Models;

namespace Test.Application.Tests.Queries
{
    public record GetAllQuestionOptionsQuery(Guid QuestionId) : IRequest<List<QuestionOption>>;

    public class GetAllQuestionOptionsQueryHandler : IRequestHandler<GetAllQuestionOptionsQuery, List<QuestionOption>>
    {
        private readonly ITestDbContext _context;

        public GetAllQuestionOptionsQueryHandler(ITestDbContext context)
        {
            _context = context;
        }

        public async Task<List<QuestionOption>> Handle(GetAllQuestionOptionsQuery request, CancellationToken cancellationToken)
        {
            return await _context.QuestionOptions
                .Where(o => o.QuestionId == request.QuestionId)
                .ToListAsync(cancellationToken);
        }
    }
}
