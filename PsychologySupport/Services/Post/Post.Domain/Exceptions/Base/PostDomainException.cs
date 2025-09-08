namespace Post.Domain.Exceptions.Base;

public class PostDomainException : Exception
{
    public PostDomainException(string message) : base(message)
    {
    }
}