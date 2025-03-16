using System;
using System.Collections.Generic;

namespace SinafProduction.Data.Entities;

public partial class Project
{
    public ulong id { get; set; }

    public string uniqueName { get; set; } = null!;

    public string name { get; set; } = null!;

    public string? description { get; set; }

    public virtual ICollection<Board> boards { get; set; } = new List<Board>();

    public virtual ICollection<User> users { get; set; } = new List<User>();
}
