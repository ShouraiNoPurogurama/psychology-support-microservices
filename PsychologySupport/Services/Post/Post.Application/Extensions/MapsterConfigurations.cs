using System.Reflection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Post.Application.ReadModels.Commands.UpdateAliasVersionReplica;
using Post.Application.ReadModels.Models;
using Post.Domain.Aggregates.Gifts.Enums;
using Post.Domain.Aggregates.Posts.Enums;
using Post.Domain.Aggregates.CategoryTags.Enums;
using Post.Domain.Aggregates.Reaction.Enums;
using Post.Domain.Aggregates.Shared.Enums;

namespace Post.Application.Extensions;

public static class MapsterConfigurations
{
    public static void RegisterMapsterConfigurations(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

        // Existing configurations
        TypeAdapterConfig<UpdateAliasVersionReplicaCommand, AliasVersionReplica>
            .NewConfig()
            .Map(dest => dest.CurrentVersionId, src => src.AliasVersionId)
            .Ignore(dest => dest.AliasId)
            .Ignore(dest => dest.LastSyncedAt);

        // Enum mappings - ensure proper conversion between domain enums and DTOs
        ConfigureEnumMappings();
    }

    private static void ConfigureEnumMappings()
    {
        // GiftTargetType mappings
        TypeAdapterConfig<string, GiftTargetType>
            .NewConfig()
            .MapWith(src => Enum.Parse<GiftTargetType>(src, true));

        TypeAdapterConfig<GiftTargetType, string>
            .NewConfig()
            .MapWith(src => src.ToString());

        // ReactionTargetType mappings
        TypeAdapterConfig<string, ReactionTargetType>
            .NewConfig()
            .MapWith(src => Enum.Parse<ReactionTargetType>(src, true));

        TypeAdapterConfig<ReactionTargetType, string>
            .NewConfig()
            .MapWith(src => src.ToString());

        // ReactionCode mappings
        TypeAdapterConfig<string, ReactionCode>
            .NewConfig()
            .MapWith(src => Enum.Parse<ReactionCode>(src, true));

        TypeAdapterConfig<ReactionCode, string>
            .NewConfig()
            .MapWith(src => src.ToString());

        // PostVisibility mappings
        TypeAdapterConfig<string, PostVisibility>
            .NewConfig()
            .MapWith(src => Enum.Parse<PostVisibility>(src, true));

        TypeAdapterConfig<PostVisibility, string>
            .NewConfig()
            .MapWith(src => src.ToString());

        // ModerationStatus mappings
        TypeAdapterConfig<string, ModerationStatus>
            .NewConfig()
            .MapWith(src => Enum.Parse<ModerationStatus>(src, true));

        TypeAdapterConfig<ModerationStatus, string>
            .NewConfig()
            .MapWith(src => src.ToString());

        // CategoryTagUpdateStatus mappings
        TypeAdapterConfig<string, CategoryTagUpdateStatus>
            .NewConfig()
            .MapWith(src => Enum.Parse<CategoryTagUpdateStatus>(src, true));

        TypeAdapterConfig<CategoryTagUpdateStatus, string>
            .NewConfig()
            .MapWith(src => src.ToString());

        // PostMediaUpdateStatus mappings
        TypeAdapterConfig<string, PostMediaUpdateStatus>
            .NewConfig()
            .MapWith(src => Enum.Parse<PostMediaUpdateStatus>(src, true));

        TypeAdapterConfig<PostMediaUpdateStatus, string>
            .NewConfig()
            .MapWith(src => src.ToString());

        // ReportedContentType mappings
        TypeAdapterConfig<string, ReportedContentType>
            .NewConfig()
            .MapWith(src => Enum.Parse<ReportedContentType>(src, true));

        TypeAdapterConfig<ReportedContentType, string>
            .NewConfig()
            .MapWith(src => src.ToString());

        // ReportReason mappings
        TypeAdapterConfig<string, ReportReason>
            .NewConfig()
            .MapWith(src => Enum.Parse<ReportReason>(src, true));

        TypeAdapterConfig<ReportReason, string>
            .NewConfig()
            .MapWith(src => src.ToString());
    }
}