using BuildingBlocks.Extensions;

namespace Media.API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMinimalJwtValidation(configuration);
            return services;
        }
    }
}
