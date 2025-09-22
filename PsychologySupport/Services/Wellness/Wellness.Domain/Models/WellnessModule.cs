using System;
using System.Collections.Generic;

namespace Wellness.Domain.Models;

public partial class WellnessModule
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual ICollection<Challenge> Challenges { get; set; } = new List<Challenge>();

    public virtual ICollection<ModuleSection> ModuleSections { get; set; } = new List<ModuleSection>();
}
