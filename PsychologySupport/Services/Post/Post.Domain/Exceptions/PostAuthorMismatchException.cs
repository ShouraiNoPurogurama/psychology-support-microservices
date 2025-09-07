using Post.Domain.Exceptions.Base;

namespace Post.Domain.Exceptions;

public class PostAuthorMismatchException : PostDomainException
{
    public PostAuthorMismatchException() : base("Chỉ có chủ sở hữu bài viết mới có thể sửa thông tin bài viết.")
    {
    }
}