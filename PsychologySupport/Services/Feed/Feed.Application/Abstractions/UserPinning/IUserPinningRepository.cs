using Feed.Domain.UserPinning;

namespace Feed.Application.Abstractions.UserPinning;

public interface IUserPinningRepository
{
    Task<bool> PinPostAsync(UserPinnedPost pinnedPost, CancellationToken ct);
    Task<bool> UnpinPostAsync(Guid aliasId, Guid postId, CancellationToken ct);
    Task<bool> IsPostPinnedAsync(Guid aliasId, Guid postId, CancellationToken ct);
    Task<IReadOnlyList<UserPinnedPost>> GetPinnedPostsAsync(Guid aliasId, CancellationToken ct);
}
