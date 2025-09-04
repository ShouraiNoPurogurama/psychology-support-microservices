using System.Reflection;
using Profile.API.Domains.DoctorProfiles.Dtos;
using Profile.API.Domains.MentalDisorders.Dtos;
using Profile.API.Domains.PatientProfiles.Dtos;
using Profile.API.Models.Public;

namespace Profile.API.Extensions;

public static class MapsterConfiguration
{
    public static void RegisterMapsterConfiguration(this IServiceCollection services)
    {
        // Scan the assembly for other mappings
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

        TypeAdapterConfig<MedicalRecord, MedicalRecordDto>
        .NewConfig()
         .Map(dest => dest.MedicalHistory, src => src.MedicalHistory)
         .Map(dest => dest.SpecificMentalDisorders, src => src.SpecificMentalDisorders.Adapt<List<SpecificMentalDisorderDto>>())
         .Map(dest => dest.MedicalHistory.PhysicalSymptoms, src => src.MedicalHistory.PhysicalSymptoms.Adapt<List<PhysicalSymptomDto>>());


        TypeAdapterConfig<MedicalHistory, MedicalHistoryDto>
            .NewConfig()
            .Map(dest => dest.SpecificMentalDisorders, src => src.SpecificMentalDisorders.Adapt<List<SpecificMentalDisorderDto>>())
            .Map(dest => dest.PhysicalSymptoms, src => src.PhysicalSymptoms.Adapt<List<PhysicalSymptomDto>>());


        TypeAdapterConfig<SpecificMentalDisorder, SpecificMentalDisorderDto>
            .NewConfig();

        TypeAdapterConfig<PhysicalSymptom, PhysicalSymptomDto>
            .NewConfig();

        TypeAdapterConfig<DoctorProfile, DoctorProfileDto>
           .NewConfig()
           .Map(dest => dest.Specialties, src => src.Specialties.Adapt<List<SpecialtyDto>>());

        TypeAdapterConfig<Specialty, SpecialtyDto>
            .NewConfig();

        TypeAdapterConfig<Industry, IndustryDto>
            .NewConfig();

        TypeAdapterConfig<Job, JobDto>
            .NewConfig();

    }
}