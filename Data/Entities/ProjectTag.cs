using System;
using System.Collections.Generic;

namespace SinafProduction.Data.Entities;

public partial class ProjectTag
{
    public byte Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
