using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Utils;

public static class DbUtils
{
    public static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true;
    
}