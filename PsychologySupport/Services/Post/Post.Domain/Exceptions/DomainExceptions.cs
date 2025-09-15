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
