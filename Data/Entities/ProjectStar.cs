using System;
using System.Collections.Generic;

namespace SinafProduction.Data.Entities;

public partial class ProjectStar
{
    public ulong ProjectId { get; set; }

    public virtual Project Project { get; set; } = null!;
}
