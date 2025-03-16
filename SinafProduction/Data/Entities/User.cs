using System;
using System.Collections.Generic;

namespace SinafProduction.Data.Entities;

public partial class User
{
    public ulong id { get; set; }

    public long? discordId { get; set; }

    public string username { get; set; } = null!;

    public string mail { get; set; } = null!;

    public bool admin { get; set; }

    public string avatar { get; set; } = null!;

    public string password { get; set; } = null!;

    public DateTime? createdAt { get; set; }

    public int version { get; set; }

    public virtual ICollection<Blog> blogs { get; set; } = new List<Blog>();

    public virtual ICollection<Project> projects { get; set; } = new List<Project>();
}
