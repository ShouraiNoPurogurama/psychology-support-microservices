using Post.Domain.Exceptions.Base;

namespace Post.Domain.Exceptions;

public class InvalidPostDataException: PostDomainException
{
    public InvalidPostDataException() : base("Thông tin bài viết không hợp lệ.")
    {
    }

    public InvalidPostDataException(string message) : base(message)
    {
    }
}