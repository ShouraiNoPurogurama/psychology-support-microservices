using Post.Domain.Exceptions.Base;

namespace Post.Domain.Exceptions;

public class InvalidRejectionReasonException : PostDomainException
{
    public InvalidRejectionReasonException() : base("Lý do từ chối duyệt không hợp lệ.")
    {
        
    }
    
    public InvalidRejectionReasonException(string message) : base(message)
    {
    }
}