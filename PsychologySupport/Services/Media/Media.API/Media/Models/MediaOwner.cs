using System;
using System.Collections.Generic;

namespace Media.API.Media.Models;

public partial class MediaOwner
{
    public Guid Id { get; set; }

    public Guid MediaId { get; set; }

    public string MediaOwnerType { get; set; } = null!;

    public string MediaOwnerId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual MediaAsset Media { get; set; } = null!;
}
