using BuildingBlocks.Data.Common;

namespace Profile.API.Pii.Models;

public partial class PersonProfile 
{
    public Guid UserId { get; set; }

    public string? FullName { get; set; }

    public string? Gender { get; set; }

    public DateOnly? BirthDate { get; set; }

    public ContactInfo ContactInfo { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}
