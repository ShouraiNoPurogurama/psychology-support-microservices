using System;
using System.Collections.Generic;
using Wellness.Domain.Abstractions;

namespace Wellness.Domain.Aggregates.ModuleSections;

public partial class WellnessModule : AuditableEntity<Guid>
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid MediaId { get; set; }

    public virtual ICollection<ModuleSection> ModuleSections { get; set; } = new List<ModuleSection>();
}
