namespace Conversation.Domain.Exceptions;

public abstract class ConversationDomainException : Exception
{
    protected ConversationDomainException(string message) : base(message) { }
    protected ConversationDomainException(string message, Exception innerException) : base(message, innerException) { }
}

public sealed class InvalidMessageContentException : ConversationDomainException
{
    public InvalidMessageContentException(string message) : base(message) { }
}

public sealed class InvalidParticipantException : ConversationDomainException
{
    public InvalidParticipantException(string message) : base(message) { }
}

public sealed class InvalidSummarizationException : ConversationDomainException
{
    public InvalidSummarizationException(string message) : base(message) { }
}

public sealed class ConversationAccessException : ConversationDomainException
{
    public ConversationAccessException(string message) : base(message) { }
}

public sealed class InvalidConversationStateException : ConversationDomainException
{
    public InvalidConversationStateException(string message) : base(message) { }
}
