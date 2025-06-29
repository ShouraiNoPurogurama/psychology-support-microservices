using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Translation;
using BuildingBlocks.Utils;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Profile.API.Exceptions;
using Profile.API.MentalDisorders.Models;
using Profile.API.PatientProfiles.Dtos;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.PatientProfiles.Features.GetPatientProfile;

public record GetPatientProfileQuery(Guid Id) : IQuery<GetPatientProfileResult>;

public record GetPatientProfileResult(GetPatientProfileDto PatientProfileDto);

public class GetPatientProfileHandler : IQueryHandler<GetPatientProfileQuery, GetPatientProfileResult>
{
    private readonly ProfileDbContext _context;
    private readonly IRequestClient<GetTranslatedDataRequest> _translationClient;

    public GetPatientProfileHandler(ProfileDbContext context, IRequestClient<GetTranslatedDataRequest> translationClient)
    {
        _context = context;
        _translationClient = translationClient;
    }

    public async Task<GetPatientProfileResult> Handle(GetPatientProfileQuery request, CancellationToken cancellationToken)
    {
        var patientProfile = await _context.PatientProfiles
            .Include(p => p.Job)
            .ThenInclude(j => j.Industry)
            .Include(p => p.MedicalHistory)
            .ThenInclude(m => m.PhysicalSymptoms)
            .Include(p => p.MedicalHistory)
            .ThenInclude(m => m.SpecificMentalDisorders)
            .Include(p => p.MedicalRecords)
            .ThenInclude(m => m.SpecificMentalDisorders)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new ProfileNotFoundException(request.Id);

        var dto = patientProfile.Adapt<GetPatientProfileDto>();

        // Tạo translationDict
        var translationDict = TranslationUtils.CreateBuilder()
            .AddEntities(dto.MedicalHistory?.SpecificMentalDisorders ?? [], nameof(SpecificMentalDisorder), x => x.Id.ToString(), x => x.MentalDisorderName, x => x.Name, x => x.Description)
            .AddEntities(dto.MedicalHistory?.PhysicalSymptoms ?? [], nameof(PhysicalSymptomDto), x => x.Id.ToString(), x => x.Name, x => x.Description)
            .AddEntities(dto.MedicalRecords.SelectMany(r => r.SpecificMentalDisorders), nameof(SpecificMentalDisorder), x => x.Id.ToString(), x => x.MentalDisorderName, x => x.Name, x => x.Description)
            .AddEntities(dto.Job?.Industry != null ? new List<Industry> { dto.Job.Industry } : new List<Industry>(), nameof(Industry), x => x.IndustryName, x => x.Description)
            .AddEntities(dto.Job != null ? new List<Job> { dto.Job } : new List<Job>(), nameof(Job), x => x.JobTitle)
            .Build();

        var response = await _translationClient.GetResponse<GetTranslatedDataResponse>(
            new GetTranslatedDataRequest(translationDict, SupportedLang.vi), cancellationToken);

        var translations = response.Message.Translations;

        // Map lại các field được dịch
        if (dto.Job is not null)
        {
            var translatedIndustry = dto.Job.Industry is not null
                ? new Industry
                {
                    Id = dto.Job.Industry.Id,
                    IndustryName = translations.GetTranslatedValue(dto.Job.Industry, x => x.IndustryName, nameof(Industry)),
                    Description = translations.GetTranslatedValue(dto.Job.Industry, x => x.Description, nameof(Industry))
                }
                : null;

            var translatedJob = new Job
            {
                Id = dto.Job.Id,
                JobTitle = translations.GetTranslatedValue(dto.Job, x => x.JobTitle, nameof(Job)),
                EducationLevel = dto.Job.EducationLevel,
                IndustryId = dto.Job.IndustryId,
                Industry = translatedIndustry
            };

            dto = dto with { Job = translatedJob };
        }



        if (dto.MedicalHistory is not null)
        {
            dto = dto with
            {
                MedicalHistory = dto.MedicalHistory with
                {
                    SpecificMentalDisorders = dto.MedicalHistory.SpecificMentalDisorders.Select(d =>
                        d with
                        {
                            MentalDisorderName = translations.GetTranslatedValue(d, x => x.MentalDisorderName, nameof(SpecificMentalDisorder)),
                            Name = translations.GetTranslatedValue(d, x => x.Name, nameof(SpecificMentalDisorder)),
                            Description = translations.GetTranslatedValue(d, x => x.Description, nameof(SpecificMentalDisorder))
                        }).ToList(),

                    PhysicalSymptoms = dto.MedicalHistory.PhysicalSymptoms.Select(s =>
                        s with
                        {
                            Name = translations.GetTranslatedValue(s, x => x.Name, nameof(PhysicalSymptomDto)),
                            Description = translations.GetTranslatedValue(s, x => x.Description, nameof(PhysicalSymptomDto))
                        }).ToList()
                }
            };
        }

        dto = dto with
        {
            MedicalRecords = dto.MedicalRecords.Select(m =>
                m with
                {
                    SpecificMentalDisorders = m.SpecificMentalDisorders.Select(d =>
                        d with
                        {
                            MentalDisorderName = translations.GetTranslatedValue(d, x => x.MentalDisorderName, nameof(SpecificMentalDisorder)),
                            Name = translations.GetTranslatedValue(d, x => x.Name, nameof(SpecificMentalDisorder)),
                            Description = translations.GetTranslatedValue(d, x => x.Description, nameof(SpecificMentalDisorder))
                        }).ToList()
                }).ToList()
        };

        return new GetPatientProfileResult(dto);
    }
}
