using System.Reflection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Post.Application.ReadModels.Commands.UpdateAliasVersionReplica;
using Post.Application.ReadModels.Models;

namespace Post.Application.Extensions;

public static class MapsterConfigurations
{
    public static void RegisterMapsterConfigurations(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

        TypeAdapterConfig<UpdateAliasVersionReplicaCommand, AliasVersionReplica>
            .NewConfig()
            .Map(dest => dest.CurrentVersionId, src => src.AliasVersionId)
            .Ignore(dest => dest.AliasId)
            .Ignore(dest => dest.LastSyncedAt);
    }
}