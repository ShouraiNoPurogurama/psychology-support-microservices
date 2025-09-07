using Post.Domain.Exceptions.Base;

namespace Post.Domain.Exceptions;

public class DeletedPostActionException : PostDomainException
{
    public DeletedPostActionException() : base("Không thể thực hiện hành động trên bài viết đã bị xóa.")
    {
        
    }
    
    public DeletedPostActionException(string message) : base(message)
    {
    }
}