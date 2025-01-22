using BuildingBlocks.Exceptions;

namespace Auth.API.Exceptions;

public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException(string? signature) : base($"Tài khoản {signature} không hợp lệ hoặc đã bị khóa.")
    {
    }
}