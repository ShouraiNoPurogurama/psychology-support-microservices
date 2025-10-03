using BuildingBlocks.CQRS;

namespace Post.Application.Features.Posts.Queries.GetPostCreationStatus;

/// <summary>
/// Query để lấy trạng thái của quá trình khởi tạo một bài viết.
/// </summary>
public record GetPostCreationStatusQuery(Guid PostId)
    : IQuery<GetPostCreationStatusResult>;

/// <summary>
/// Kết quả trả về cho client, chứa trạng thái hiện tại của bài viết.
/// </summary>
public record GetPostCreationStatusResult(
    Guid PostId,
    string Status, // vd: "Creating", "Finalized", "CreationFailed"
    string? Reason = null // Lý do thất bại nếu có
);
