using Post.Domain.Aggregates.Posts.Enums;
using Post.Domain.Exceptions;

namespace Post.Domain.Aggregates.Posts.ValueObjects;

public sealed record AuthorInfo
{
    public Guid AliasId { get; init; }
    public Guid AliasVersionId { get; init; }

    // EF Core materialization
    private AuthorInfo() { }

    // Ctor kín – chỉ factory được phép gọi
    private AuthorInfo(Guid aliasId, Guid? aliasVersionId)
    {
        AliasId = aliasId;
        AliasVersionId = aliasVersionId ?? Guid.Empty;
    }

    public static AuthorInfo Create(Guid aliasId, Guid? aliasVersionId = null)
    {
        if (aliasId == Guid.Empty)
            throw new InvalidPostDataException("ID của tác giả không hợp lệ.");

        return new AuthorInfo(aliasId, aliasVersionId);
    }

    public bool HasVersionInfo => AliasVersionId != Guid.Empty;
}

