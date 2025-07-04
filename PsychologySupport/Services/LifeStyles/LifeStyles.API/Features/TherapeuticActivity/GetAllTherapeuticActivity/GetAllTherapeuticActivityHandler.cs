﻿using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Translation;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.TherapeuticActivity.GetAllTherapeuticActivity;

public record GetAllTherapeuticActivitiesQuery(
    int PageIndex,
    int PageSize,
    string? Search,
    IntensityLevel? IntensityLevel,
    ImpactLevel? ImpactLevel
) : IQuery<GetAllTherapeuticActivitiesResult>;

public record GetAllTherapeuticActivitiesResult(PaginatedResult<TherapeuticActivityDto> TherapeuticActivities);

public class GetAllTherapeuticActivityHandler
    : IQueryHandler<GetAllTherapeuticActivitiesQuery, GetAllTherapeuticActivitiesResult>
{
    private readonly LifeStylesDbContext _context;
    private readonly IRequestClient<GetTranslatedDataRequest> _translationClient;

    public GetAllTherapeuticActivityHandler(
        LifeStylesDbContext context,
        IRequestClient<GetTranslatedDataRequest> translationClient
    )
    {
        _context = context;
        _translationClient = translationClient;
    }

    public async Task<GetAllTherapeuticActivitiesResult> Handle(
        GetAllTherapeuticActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize;
        var pageIndex = request.PageIndex;

        var query = _context.TherapeuticActivities.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(ea => ea.Name.Contains(request.Search));
        }

        if (request.IntensityLevel.HasValue)
        {
            query = query.Where(ea => ea.IntensityLevel == request.IntensityLevel.Value);
        }

        if (request.ImpactLevel.HasValue)
        {
            query = query.Where(ea => ea.ImpactLevel == request.ImpactLevel.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var activities = await query
            .OrderBy(ea => ea.Name)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<TherapeuticActivityDto>()
            .ToListAsync(cancellationToken);

        var translatedActivities = await activities.TranslateEntitiesAsync(
            nameof(Models.TherapeuticActivity), _translationClient, a => a.Id.ToString(), cancellationToken,
            a => a.Name,
            a => a.Description,
            a => a.IntensityLevel,
            a => a.ImpactLevel,
            a => a.TherapeuticTypeName,
            a => a.Instructions
        );

        var result = new PaginatedResult<TherapeuticActivityDto>(
            pageIndex,
            pageSize,
            totalCount,
            translatedActivities
        );

        return new GetAllTherapeuticActivitiesResult(result);
    }
}
