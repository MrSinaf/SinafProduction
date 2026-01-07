using System;
using System.Collections.Generic;

namespace SinafProduction.Data.Entities;

public partial class ProjectRepository
{
    public ulong Id { get; set; }

    public ulong ProjectId { get; set; }

    public string Repository { get; set; } = null!;

    public string Branch { get; set; } = null!;

    public string Commit { get; set; } = null!;

    public uint Added { get; set; }

    public uint Removed { get; set; }

    public uint Modified { get; set; }

    public DateTime Update { get; set; }

    public virtual Project Project { get; set; } = null!;
}
