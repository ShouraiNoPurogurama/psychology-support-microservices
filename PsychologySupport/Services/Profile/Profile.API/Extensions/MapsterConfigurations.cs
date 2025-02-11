using System.Reflection;
using Mapster;
using Profile.API.Dtos;
using Profile.API.Models;

namespace Profile.API.Extensions;

public static class MapsterConfiguration
{
    public static void RegisterMapsterConfiguration(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        
        TypeAdapterConfig<DoctorProfileDto, DoctorProfile>.NewConfig()
            .IgnoreNullValues(true);
        TypeAdapterConfig<PatientProfileDto, PatientProfile>.NewConfig()
           .IgnoreNullValues(true);
    }
}