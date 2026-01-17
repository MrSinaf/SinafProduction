using System;
using System.Collections.Generic;

namespace SinafProduction.Data.Entities;

public partial class Project
{
    public ulong Id { get; set; }

    public string UniqueName { get; set; } = null!;

    public string Name { get; set; } = null!;

    public byte TagId { get; set; }

    public string Description { get; set; } = null!;

    public string? Markdown { get; set; }

    public DateOnly CreatedAt { get; set; }

    public virtual ICollection<ProjectRepository> ProjectRepositories { get; set; } = new List<ProjectRepository>();

    public virtual ProjectStar? ProjectStar { get; set; }

    public virtual ProjectTag Tag { get; set; } = null!;
}
