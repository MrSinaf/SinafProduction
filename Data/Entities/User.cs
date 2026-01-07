using System;
using System.Collections.Generic;

namespace SinafProduction.Data.Entities;

public partial class User
{
    public ulong Id { get; set; }

    public string UniqueName { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool Admin { get; set; }

    public string Password { get; set; } = null!;

    public uint Version { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
