using System;
using System.Collections.Generic;

namespace SinafProduction.Data.Entities;

public partial class Blog
{
    public uint id { get; set; }

    public ulong authorId { get; set; }

    public string title { get; set; } = null!;

    public string description { get; set; } = null!;

    public string content { get; set; } = null!;

    public bool isPublic { get; set; }

    public DateTime? createdAt { get; set; }

    public virtual User author { get; set; } = null!;
}
