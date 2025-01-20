using BuildingBlocks.Exceptions;

namespace Auth.API.Exceptions;

public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException(string? email) : base($"Tài khoản {email} không hợp lệ hoặc đã bị khóa.")
    {
    }
}