﻿using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Profile.API.PatientProfiles.Dtos;
using Profile.API.PatientProfiles.Enum;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.PatientProfiles.Features.GetAllPatientProfiles;

public record GetAllPatientProfilesQuery(
        [FromQuery] int PageIndex,
        [FromQuery] int PageSize,
        [FromQuery] string? Search = "", // FullName,PhoneNumber
        [FromQuery] string? SortBy = "fullname", // sort fullname or createdat(MedicalRecord)
        [FromQuery] string? SortOrder = "asc", // asc or desc
        [FromQuery] UserGender? Gender = null, // filter
        [FromQuery] MedicalRecordStatus? MedicalRecordStatusStatus = null, // filter Processing,Done
        [FromQuery] PersonalityTrait? PersonalityTrait = null) : IQuery<GetAllPatientProfilesResult>;
  
public record GetAllPatientProfilesResult(PaginatedResult<GetPatientProfileDto> PaginatedResult);

public class GetAllPatientProfilesHandler : IQueryHandler<GetAllPatientProfilesQuery, GetAllPatientProfilesResult>
{
    private readonly ProfileDbContext _context;

    public GetAllPatientProfilesHandler(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllPatientProfilesResult> Handle(GetAllPatientProfilesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize;
        var pageIndex = request.PageIndex;

        IQueryable<PatientProfile> query = _context.PatientProfiles
            .Include(p => p.MedicalRecords)
                .ThenInclude(m => m.SpecificMentalDisorders)
            .Include(p => p.MedicalHistory)
                .ThenInclude(m => m.SpecificMentalDisorders)
            .Include(p => p.MedicalHistory)
                .ThenInclude(m => m.PhysicalSymptoms)
            .Include(p => p.Job);

        // Filter 
        if (request.Gender.HasValue)
        {
            query = query.Where(p => p.Gender == request.Gender.Value);
        }

        if (request.PersonalityTrait.HasValue)
        {
            query = query.Where(p => p.PersonalityTraits == request.PersonalityTrait.Value);
        }

        if (request.MedicalRecordStatusStatus.HasValue)
        {
            query = query.Where(p => p.MedicalRecords.Any(mr => mr.Status == request.MedicalRecordStatusStatus.Value));
        }




        // Search 
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(p => p.FullName.Contains(request.Search) || p.ContactInfo.PhoneNumber.Contains(request.Search));
        }

        // Sorting 
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            if (request.SortBy.ToLower() == "fullname")
            {
                query = request.SortOrder.ToLower() == "asc"
                    ? query.OrderBy(p => p.FullName)
                    : query.OrderByDescending(p => p.FullName);
            }
        }
        else if (request.SortBy.ToLower() == "createdat") 
        {
            query = request.SortOrder.ToLower() == "asc"
                ? query.OrderBy(p => p.MedicalRecords.Min(mr => mr.CreatedAt))
                : query.OrderByDescending(p => p.MedicalRecords.Max(mr => mr.CreatedAt));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var patientProfiles = await query
            .Skip((pageIndex - 1) * pageSize) 
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var result = new PaginatedResult<GetPatientProfileDto>(pageIndex, pageSize, totalCount,
            patientProfiles.Adapt<IEnumerable<GetPatientProfileDto>>());

        return new GetAllPatientProfilesResult(result);
    }

}