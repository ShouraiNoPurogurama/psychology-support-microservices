
using Alias.API.Aliases.Exceptions.DomainExceptions.Base;

namespace Alias.API.Aliases.Exceptions.DomainExceptions;

public class InvalidAliasDataException : AliasDomainException
{
    public InvalidAliasDataException(string message) : base(message) { }
}

public class AliasNotFoundException : AliasDomainException
{
    public AliasNotFoundException(string message = "Alias not found") : base(message) { }
}

public class AliasConflictException : AliasDomainException
{
    public AliasConflictException(string message = "Alias already exists") : base(message) { }
}

public class AliasSuspendedException : AliasDomainException
{
    public AliasSuspendedException(string message = "Cannot perform action on suspended alias") : base(message) { }
}

public class AliasVersionLimitExceededException : AliasDomainException
{
    public AliasVersionLimitExceededException(string message = "Maximum alias versions reached") : base(message) { }
}

public class InvalidAliasAuditActionException : AliasDomainException
{
    public InvalidAliasAuditActionException(string message = "Alias audit action is invalid") : base(message) { }
}
