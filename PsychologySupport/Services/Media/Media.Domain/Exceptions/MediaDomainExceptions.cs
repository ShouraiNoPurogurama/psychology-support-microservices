namespace Media.Domain.Exceptions;

public class MediaDomainException : Exception
{
    public MediaDomainException(string message) : base(message) { }
    
    public MediaDomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class MediaNotFoundException : MediaDomainException
{
    public MediaNotFoundException(string message) : base(message) { }
}

public class MediaProcessingException : MediaDomainException
{
    public MediaProcessingException(string message) : base(message) { }
    
    public MediaProcessingException(string message, Exception innerException) : base(message, innerException) { }
}

public class MediaModerationException : MediaDomainException
{
    public MediaModerationException(string message) : base(message) { }
}

public class MediaOwnershipException : MediaDomainException
{
    public MediaOwnershipException(string message) : base(message) { }
}
