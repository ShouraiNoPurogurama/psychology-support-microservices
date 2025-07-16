using BuildingBlocks.CQRS;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Test.Application.Data;
using Test.Application.Dtos;
using Test.Application.ServiceContracts;
using Test.Domain.Enums;
using Test.Domain.Models;
using Test.Domain.ValueObjects;

namespace Test.Application.TestOutput.Commands;

public class CreateTestResultCommand : ICommand<CreateTestResultResult>
{
    public Guid PatientId { get; set; }
    public Guid TestId { get; set; }
    public List<Guid> SelectedOptionIds { get; set; } = new();
}

public record CreateTestResultResult(TestResultDto TestResult);

public class CreateTestResultHandler(
    ITestDbContext dbContext,
    IPublisher publisher,
    IAIClient aiClient
)
    : ICommandHandler<CreateTestResultCommand, CreateTestResultResult>
{
    public async Task<CreateTestResultResult> Handle(CreateTestResultCommand request, CancellationToken cancellationToken)
    {
        var selectedOptions = await dbContext.QuestionOptions
            .Where(o => request.SelectedOptionIds.Contains(o.Id))
            .ToListAsync(cancellationToken);

        // List questions by QuestionId
        var questionIds = selectedOptions.Select(o => o.QuestionId).Distinct().ToList();
        var questions = await dbContext.TestQuestions
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

        var DASS21Response = await aiClient.GetDASS21RecommendationsAsync(
            request.PatientId.ToString(),
            depressionScore,
            anxietyScore,
            stressScore
        );

        var recommendationVO = Recommendation.Create(
            DASS21Response.Recommendation.Overview,
            DASS21Response.Recommendation.EmotionAnalysis,
            DASS21Response.Recommendation.PersonalizedSuggestions
                .Select(s => new PersonalizedSuggestion(s.Title, s.Description, s.Tips, s.Reference))
                .ToList(),
            DASS21Response.Recommendation.Closing
        );

        
        var testResult = TestResult.Create(
            request.PatientId,
            request.TestId,
            depressionScore,
            anxietyScore,
            stressScore,
            severityLevel,
            recommendationVO,
            DASS21Response.ProfileNickname,
            DASS21Response.ProfileDescription,
            selectedOptions
        );

        await dbContext.TestResults.AddAsync(testResult, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        //Publish notification 
        // await publisher.Publish(new TestResultCreatedEvent(testResult.Id, request.SelectedOptionIds), cancellationToken);

        var result = new CreateTestResultResult(new TestResultDto
        (
            Id: testResult.Id,
            TestId: testResult.TestId,
            PatientId: testResult.PatientId,
            TakenAt: testResult.TakenAt,
            DepressionScore: depressionScore,
            AnxietyScore: anxietyScore,
            StressScore: stressScore,
            SeverityLevel: severityLevel,
            Recommendation: recommendationVO,
            PatientName: DASS21Response.PatientName,
            PatientAge: DASS21Response.PatientAge,
            ProfileNickname: DASS21Response.ProfileNickname,
            ProfileDescription: DASS21Response.ProfileDescription,
            ProfileHighlights: DASS21Response.ProfileHighlights
        ));
        return result;
    }

    private SeverityLevel DetermineSeverity(Score depression, Score anxiety, Score stress)
    {
        var totalScore = depression.Value + anxiety.Value + stress.Value;

        if (totalScore >= 30) return SeverityLevel.Severe;
        if (totalScore >= 15) return SeverityLevel.Moderate;
        return SeverityLevel.Mild;
    }
}