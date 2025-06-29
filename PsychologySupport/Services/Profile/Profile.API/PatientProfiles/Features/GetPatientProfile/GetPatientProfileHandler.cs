using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Translation;
using BuildingBlocks.Utils;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Profile.API.Exceptions;
using Profile.API.MentalDisorders.Models;
using Profile.API.PatientProfiles.Dtos;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.PatientProfiles.Features.GetPatientProfile;

public record GetPatientProfileQuery(Guid Id) : IQuery<GetPatientProfileResult>;

public record GetPatientProfileResult(GetPatientProfileDto PatientProfileDto);

public class GetPatientProfileHandler(
    ProfileDbContext context,
    IRequestClient<GetTranslatedDataRequest> translationClient
) : IQueryHandler<GetPatientProfileQuery, GetPatientProfileResult>
{
    public async Task<GetPatientProfileResult> Handle(GetPatientProfileQuery request, CancellationToken cancellationToken)
    {
        var patientProfile = await context.PatientProfiles
            .Include(p => p.Job).ThenInclude(j => j.Industry)
            .Include(p => p.MedicalHistory).ThenInclude(m => m.PhysicalSymptoms)
            .Include(p => p.MedicalHistory).ThenInclude(m => m.SpecificMentalDisorders)
            .Include(p => p.MedicalRecords).ThenInclude(m => m.SpecificMentalDisorders)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new ProfileNotFoundException(request.Id);

        var dto = patientProfile.Adapt<GetPatientProfileDto>();

        //Tạo dict dịch
        var translationDict = TranslationUtils.CreateBuilder()
            .AddEntities([dto], nameof(PatientProfile), x => x.PersonalityTraits)
            .AddStrings(dto.Job is not null ? [dto.Job] : [], nameof(Job))
            .AddStrings(dto.MedicalHistory?.PhysicalSymptoms.Select(x => x.Name) ?? [], nameof(PhysicalSymptom))
            .AddStrings(dto.MedicalHistory?.PhysicalSymptoms.Select(x => x.Description) ?? [], nameof(PhysicalSymptom))
            .AddStrings(dto.MedicalHistory?.SpecificMentalDisorders.Select(x => x.Name) ?? [], nameof(SpecificMentalDisorder))
            .AddStrings(dto.MedicalHistory?.SpecificMentalDisorders.Select(x => x.Description) ?? [], nameof(SpecificMentalDisorder))
            .AddStrings(dto.MedicalRecords.SelectMany(r => r.SpecificMentalDisorders).Select(x => x.Name), nameof(SpecificMentalDisorder))
            .AddStrings(dto.MedicalRecords.SelectMany(r => r.SpecificMentalDisorders).Select(x => x.Description), nameof(SpecificMentalDisorder))
            .Build();

        var response = await translationClient.GetResponse<GetTranslatedDataResponse>(
            new GetTranslatedDataRequest(translationDict, SupportedLang.vi),
            cancellationToken);

        var translations = response.Message.Translations;

        //Gán lại bản dịch
        var translated = dto with
        {
            PersonalityTraits = translations.GetTranslatedValue(dto, x => x.PersonalityTraits, nameof(PatientProfile)),

            Job = dto.Job is not null
                ? translations.MapTranslatedStrings([dto.Job], nameof(Job)).FirstOrDefault()
                : null,
            
            MedicalHistory = dto.MedicalHistory is null ? null : dto.MedicalHistory with
            {
                PhysicalSymptoms = dto.MedicalHistory.PhysicalSymptoms.Select(symptom => symptom with
                {
                    Name = translations.GetTranslatedValue(symptom, x => x.Name, nameof(PhysicalSymptom)),
                    Description = translations.GetTranslatedValue(symptom, x => x.Description, nameof(PhysicalSymptom))
                }).ToList(),

                SpecificMentalDisorders = dto.MedicalHistory.SpecificMentalDisorders.Select(d => d with
                {
                    Name = translations.GetTranslatedValue(d, x => x.Name, nameof(SpecificMentalDisorder)),
                    Description = translations.GetTranslatedValue(d, x => x.Description, nameof(SpecificMentalDisorder))
                }).ToList()
            },

            MedicalRecords = dto.MedicalRecords.Select(r => r with
            {
                SpecificMentalDisorders = r.SpecificMentalDisorders.Select(d => d with
                {
                    Name = translations.GetTranslatedValue(d, x => x.Name, nameof(SpecificMentalDisorder)),
                    Description = translations.GetTranslatedValue(d, x => x.Description, nameof(SpecificMentalDisorder))
                }).ToList()
            }).ToList()
        };

        return new GetPatientProfileResult(translated);
    }
}
