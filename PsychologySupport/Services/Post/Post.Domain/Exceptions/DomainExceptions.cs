using Post.Domain.Exceptions.Base;

namespace Post.Domain.Exceptions;

// Comment Exceptions
public class InvalidCommentDataException : PostDomainException
{
    public InvalidCommentDataException(string message) : base(message) { }
}

public class CommentAuthorMismatchException : PostDomainException
{
    public CommentAuthorMismatchException(string message = "Chỉ tác giả mới có thể chỉnh sửa bình luận này.") : base(message) { }
}

public class DeletedCommentActionException : PostDomainException
{
    public DeletedCommentActionException(string message) : base(message) { }
}

// Reaction Exceptions
public class InvalidReactionDataException : PostDomainException
{
    public InvalidReactionDataException(string message) : base(message) { }
}

public class ReactionAuthorMismatchException : PostDomainException
{
    public ReactionAuthorMismatchException(string message = "Chỉ người tạo phản ứng mới có thể chỉnh sửa.") : base(message) { }
}

public class DeletedReactionActionException : PostDomainException
{
    public DeletedReactionActionException(string message) : base(message) { }
}

// Gift Exceptions
public class InvalidGiftDataException : PostDomainException
{
    public InvalidGiftDataException(string message) : base(message) { }
}

public class GiftAuthorMismatchException : PostDomainException
{
    public GiftAuthorMismatchException(string message = "Chỉ người gửi quà mới có thể chỉnh sửa.") : base(message) { }
}

public class DeletedGiftActionException : PostDomainException
{
    public DeletedGiftActionException(string message) : base(message) { }
}

// CategoryTag Exceptions
public class InvalidCategoryTagDataException : PostDomainException
{
    public InvalidCategoryTagDataException(string message) : base(message) { }
}

public class CategoryTagNotFoundException : PostDomainException
{
    public CategoryTagNotFoundException(string message = "Danh mục không tồn tại.") : base(message) { }
}

public class CategoryTagConflictException : PostDomainException
{
    public CategoryTagConflictException(string message) : base(message) { }
}
