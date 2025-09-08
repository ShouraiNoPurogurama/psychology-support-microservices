using Post.Domain.Exceptions.Base;

namespace Post.Domain.Exceptions;

public class InvalidPostModerationStateException : PostDomainException
{
    public InvalidPostModerationStateException() : base("Không thể thay đổi trạng thái duyệt bài.")
    {
        
    }
    
    public InvalidPostModerationStateException(string message) : base(message)
    {
    }
}