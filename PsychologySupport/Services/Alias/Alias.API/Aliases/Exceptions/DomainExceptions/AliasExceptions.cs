
using Alias.API.Aliases.Exceptions.DomainExceptions.Base;

namespace Alias.API.Aliases.Exceptions.DomainExceptions;

public class InvalidAliasDataException : AliasDomainException
{
    public InvalidAliasDataException(string message) : base(message) { }
}

public class AliasNotFoundException : AliasDomainException
{
    public AliasNotFoundException(string message = "Không tìm thấy bí danh.") : base(message) { }
}

public class AliasConflictException : AliasDomainException
{
    public AliasConflictException(string message = "Bí danh đã tồn tại.") : base(message) { }
}

public class AliasSuspendedException : AliasDomainException
{
    public AliasSuspendedException(string message = "Không thể thực hiện thao tác này.") : base(message) { }
}

public class AliasVersionLimitExceededException : AliasDomainException
{
    public AliasVersionLimitExceededException(string message = "Đã đạt giới hạn phiên bản.") : base(message) { }
}

public class InvalidAliasAuditActionException : AliasDomainException
{
    public InvalidAliasAuditActionException(string message = "Thao tác không hợp lệ.") : base(message) { }
}
