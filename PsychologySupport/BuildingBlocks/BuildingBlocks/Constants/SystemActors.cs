namespace BuildingBlocks.Constants;

public static class SystemActors
{
    /// <summary>
    /// Represents an action performed automatically by the system,
    /// such as auto-approval, system jobs, or bypass operations.
    /// </summary>
    public static readonly Guid SystemUUID = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");
    
    public static readonly Guid SystemModeratorUUID = new Guid("00000000-0000-0000-0000-000000000001");
    
    
}