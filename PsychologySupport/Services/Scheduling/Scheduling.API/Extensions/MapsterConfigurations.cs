using Mapster;
using Scheduling.API.Dtos;
using Scheduling.API.Models;
using System.Reflection;

namespace Scheduling.API.Extensions
{
    public static class MapsterConfigurations
    {
        public static void RegisterMapsterConfiguration(this IServiceCollection services)
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

           TypeAdapterConfig<Booking, BookingDto>
            .NewConfig();
            TypeAdapterConfig<Session, SessionDto>
            .NewConfig();
        }
    }
}
