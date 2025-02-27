using MediatR;
using Microsoft.EntityFrameworkCore;
using Test.Application.Data;
using Test.Domain.Enums;
using Test.Domain.Events;
using Test.Domain.Models;
using Test.Domain.ValueObjects;

namespace Test.Application.TestOutput.Commands
{
    public class CreateTestResultCommand : IRequest<Guid>
    {
        public Guid PatientId { get; set; }
        public Guid TestId { get; set; }
        public List<Guid> SelectedOptionIds { get; set; } = new();
    }

    public class CreateTestResultHandler : IRequestHandler<CreateTestResultCommand, Guid>
    {
        private readonly ITestDbContext _dbContext;
        private readonly IPublisher _publisher;

        public CreateTestResultHandler(ITestDbContext dbContext, IPublisher publisher)
        {
            _dbContext = dbContext;
            _publisher = publisher;
        }

        public async Task<Guid> Handle(CreateTestResultCommand request, CancellationToken cancellationToken)
        {
            var selectedOptions = await _dbContext.QuestionOptions
                .Where(o => request.SelectedOptionIds.Contains(o.Id))
                .ToListAsync(cancellationToken);

            // List questions by QuestionId
            var questionIds = selectedOptions.Select(o => o.QuestionId).Distinct().ToList();
            var questions = await _dbContext.TestQuestions
                .Where(q => questionIds.Contains(q.Id))
                .ToListAsync(cancellationToken);

            // Map QuestionId -> Order
            var questionOrderMap = questions.ToDictionary(q => q.Id, q => q.Order);

            // DASS-21
            var depressionScore = new Score(selectedOptions
                .Where(o => new[] { 3, 5, 10, 13, 16, 17, 21 }.Contains(questionOrderMap[o.QuestionId]))
                .Sum(o => (int)o.OptionValue));

            var anxietyScore = new Score(selectedOptions
                .Where(o => new[] { 2, 4, 7, 9, 15, 19, 20 }.Contains(questionOrderMap[o.QuestionId]))
                .Sum(o => (int)o.OptionValue));

            var stressScore = new Score(selectedOptions
                .Where(o => new[] { 1, 6, 8, 11, 12, 14, 18 }.Contains(questionOrderMap[o.QuestionId]))
                .Sum(o => (int)o.OptionValue));

            var severityLevel = DetermineSeverity(depressionScore, anxietyScore, stressScore);


            var testResult = TestResult.Create(
                request.PatientId,
                request.TestId,
                depressionScore,
                anxietyScore,
                stressScore,
                severityLevel,
                "Recommendation goes here",
                request.SelectedOptionIds
            );

            await _dbContext.TestResults.AddAsync(testResult, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(new TestResultCreatedEvent(testResult.Id, request.SelectedOptionIds), cancellationToken);

            return testResult.Id;
        }

        private SeverityLevel DetermineSeverity(Score depression, Score anxiety, Score stress)
        {
            int totalScore = depression.Value + anxiety.Value + stress.Value;

            if (totalScore >= 30) return SeverityLevel.Severe;
            if (totalScore >= 15) return SeverityLevel.Moderate;
            return SeverityLevel.Mild;
        }


    }
}
