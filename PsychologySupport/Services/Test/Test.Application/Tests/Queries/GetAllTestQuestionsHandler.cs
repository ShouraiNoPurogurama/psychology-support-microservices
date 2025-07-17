using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Translation;
using BuildingBlocks.Pagination;
using BuildingBlocks.Utils;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Test.Application.Data;
using Test.Application.Dtos;
using Test.Domain.Models;

namespace Test.Application.Tests.Queries;

public record GetAllTestQuestionsQuery(Guid TestId, PaginationRequest PaginationRequest) : IQuery<GetAllTestQuestionsResult>;

public record GetAllTestQuestionsResult(PaginatedResult<TestQuestionDto> TestQuestions);

public class GetAllTestQuestionsHandler : IQueryHandler<GetAllTestQuestionsQuery, GetAllTestQuestionsResult>
{
    private readonly ITestDbContext _context;
    private readonly IRequestClient<GetTranslatedDataRequest> _translationClient;

    public GetAllTestQuestionsHandler(ITestDbContext context, IRequestClient<GetTranslatedDataRequest> translationClient)
    {
        _context = context;
        _translationClient = translationClient;
    }

    public async Task<GetAllTestQuestionsResult> Handle(GetAllTestQuestionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TestQuestions.AsNoTracking()
            .Where(q => q.TestId == request.TestId)
            .OrderBy(q => q.Order)
            .ProjectToType<TestQuestionDto>();
        
        var totalCount = await query.CountAsync(cancellationToken);

        var rawQuestions = await query
            .Skip((request.PaginationRequest.PageIndex - 1) * request.PaginationRequest.PageSize)
            .Take(request.PaginationRequest.PageSize)
            .Select(q => new TestQuestionDto(
                q.Id,
                q.Order,
                q.Content,
                q.Options.OrderBy(o => o.OptionValue).ToList() //sort trực tiếp
            ))
            .ToListAsync(cancellationToken);

        var translationDict = TranslationUtils.CreateBuilder()
            .AddEntities(rawQuestions, nameof(TestQuestion), q => q.Content)
            .AddEntities(rawQuestions.SelectMany(q => q.Options), nameof(QuestionOption), o => o.Content)
            .Build();

        //Gọi translation service
        var translationResponse = await _translationClient.GetResponse<GetTranslatedDataResponse>(
            new GetTranslatedDataRequest(translationDict, SupportedLang.vi), cancellationToken);

        var translations = translationResponse.Message.Translations;

        //CLEAN MAPPING - Dùng batch processing
        var translatedQuestions = rawQuestions.Select(q =>
            {
                var translatedQuestion =
                    translations.MapTranslatedProperties(q, nameof(TestQuestion), id: q.Id.ToString(), q => q.Content);
                var translatedOptions =
                    translations.MapTranslatedPropertiesForCollection(q.Options, nameof(QuestionOption), o => o.Content);

                return new TestQuestionDto(q.Id, q.Order, translatedQuestion.Content, translatedOptions.ToList());
            })
            .ToList();

        var paginatedResult = new PaginatedResult<TestQuestionDto>(
            request.PaginationRequest.PageIndex,
            request.PaginationRequest.PageSize,
            totalCount,
            translatedQuestions
        );

        return new GetAllTestQuestionsResult(paginatedResult);
    }
}