using System.Reflection;
using Mapster;
using Profile.API.DoctorProfiles.Dtos;
using Profile.API.PatientProfiles.Dtos;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.Extensions;

public static class MapsterConfiguration
{
    public static void RegisterMapsterConfiguration(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
    }
}